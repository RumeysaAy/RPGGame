using System;
using GameDevTV.Utils;
using RPG.Attributes;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using UnityEngine;

// Oyuncunun herhangi bir düşmana ne kadar yakın olduğunu belirleyeceğiz.
// Takip etme menzili: chaseDistance

// oyuncunun kaçabilmesi için düşmanın oyuncudan yavaş olması lazım
// düşman nav mesh agent hızı düşürülmeli

// menzil dışına çıktığımızda bizi kovalamaktan vazgeçmeli

// menzil içindeyse saldırsın

// düşmana (Enemy) saldırması için
// düşmana tıklayınca saldırır

// kovalama menzilindeysek, savaşmaya başlamak istediğimizi söylemek istiyoruz

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        // düşmanların hangi mesafeden kovalamaları gerektiği belirtilecek
        [SerializeField] float chaseDistance = 5f; // takip mesafesi
        [SerializeField] float suspicionTime = 3f; // bekleme/şüphe zamanı
        [SerializeField] float aggroCooldownTime = 5f;
        [SerializeField] PatrolPath patrolPath; // devriye yolu
        [SerializeField] float waypointTolerance = 1f;
        [SerializeField] float waypointDwellTime = 3f; // ara nokta bekleme süresi
        [Range(0, 1)][SerializeField] float patrolSpeedFraction = 0.2f; // 0-1
        [SerializeField] float shoutDistance = 5f;
        // navMeshAgent.speed = maxSpeed * speedFraction;
        // maxSpeed * patrolSpeedFraction = düşmanın devriye gezdiği zamanki hızı

        Fighter fighter;
        Health health;
        Mover mover;
        GameObject player;

        // Düşmanlar kovaladıktan sonra eski yerlerine geri dönmeleri için
        LazyValue<Vector3> guardPosition; // dönecekleri yer

        /*
        Oyuncuyu menzil dışına kadar kovaladıktan sonra beklesin, sonra başlangıç noktasına geri dönsün
        Muhafızın şimdi düşmanı en son ne zaman ya da ne kadar zaman önce gördüğünü hatırlaması gerekir.
        Oyuncuyu son gördüğünden bu yana geçen sürenin çok yüksek olması gerektiğini varsayabiliriz
        */
        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceArrivedAtWaypoint = Mathf.Infinity; // Noktalarda beklesin
        float timeSinceAggrevated = Mathf.Infinity; // Düşmana saldırdığımızda düşmanda koşarak gelerek bize saldırsın
        int currentWaypointIndex = 0; // devriye noktasının indeksi

        private void Awake()
        {
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            player = GameObject.FindWithTag("Player");

            guardPosition = new LazyValue<Vector3>(GetGuardPosition);
        }

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }

        private void Start()
        {
            guardPosition.ForceInit();
        }

        // oyuncunun takip mesafesi içinde olup olmadığı
        // her karede kontrol edilmeli

        // etiket ile player/oyuncuyu yakalayacağız
        private void Update()
        {

            // AI(düşman) ya da player ölmüş mü
            if (health.IsDead())
            {
                return;
            }

            if (IsAggrevated() && fighter.CanAttack(player))
            {
                // oyuncuyu gördüğünde saldırıya geçer
                // print(gameObject.name + " Should Chase"); // düşman

                AttackBehaviour(); // menzilin içerisindeyse düşman oyuncuya saldıracak
            }
            // Şüphe durumu - bekleme durumu
            else if (timeSinceLastSawPlayer < suspicionTime) // eğer saldırmazsak, şüpheli oluruz
            {
                // oyuncuyu son gördüğümüzden bu yana geçen süre, şüphe süresinden kısa ise
                // şüphe durumuna geçilir
                SuspicionBehaviour();

            }
            // düşman belirli bir süre önce uzaklaştığı için
            // artık şüphelenme zamanı gelmediyse devriye yoluna geri döner
            else // menzil dışında
            {
                // Muhafızın etrafındaki devriye yolunu takip etmesini için
                PatrolBehaviour();
                // hareket eylemi başladığında otomatik olarak saldırı eylemi iptal edilir
            }

            UpdateTimers();

        }

        public void Aggrevate()
        {
            // 
            timeSinceAggrevated = 0;

        }

        private void UpdateTimers()
        {
            // Oyuncuyu son gördüğümüzden bu yana geçen süre
            timeSinceLastSawPlayer += Time.deltaTime;
            // her karede karenin aldığı miktar kadar artar

            // ara noktaya vardığımızdan bu yana geçen süre
            timeSinceArrivedAtWaypoint += Time.deltaTime;

            //
            timeSinceAggrevated += Time.deltaTime;
        }

        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition.value;

            // 
            if (patrolPath != null) // devriye yolu varsa
            {
                if (AtWaypoint()) // noktadaysa
                {
                    timeSinceArrivedAtWaypoint = 0;
                    // diğer bir noktaya gider
                    CycleWaypoint();

                }
                nextPosition = GetCurrentWaypoint();

            }
            // Noktaya varıştan bu yana geçen süre ara nokta bekleme süresinden büyükse o zaman bir sonraki ara noktaya geçsin
            if (timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
                // Muhafızın devriye yolunu takip etmesini için
                mover.StartMoveAction(nextPosition, patrolSpeedFraction); // başladığı noktaya geri dönsün
            }

        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            // devriye noktasında mı?
            return distanceToWaypoint < waypointTolerance;
        }

        private void CycleWaypoint()
        {
            // indeks verilir ve bir sonraki indeks geri dönderilir
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
        }

        private Vector3 GetCurrentWaypoint()
        {
            // indeks verilir, konum geri dönderilir
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        private void SuspicionBehaviour() // şüphe durumu
        {
            GetComponent<ActionScheduler>().CancelCurrentAction(); // mevcut durum iptal
        }

        private void AttackBehaviour()
        {
            // oyuncuyu son gördüğümüzden bu yana geçen süreyi güncelleyebiliriz
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);

            // yakındaki düşmanlarında saldırması için
            // Bir muhafıza saldırıldığında arkadaşlarıda saldırsın
            // 5 metre yakınımdaki tüm muhafızlar
            AggrevateNearbyEnemies();
        }

        private void AggrevateNearbyEnemies()
        {
            // SphereCastAll: yarıçap içerisindeki tüm düşmanlar
            // oyuncunun etrafında bir çember oluşturuyorum
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, shoutDistance, Vector3.up, 0);
            foreach (RaycastHit hit in hits)
            {
                // çemberin içerisindeki düşmanlar tespit edilir
                AIController ai = hit.collider.GetComponent<AIController>();
                if (ai == null)
                {
                    continue;
                }

                ai.Aggrevate(); // çemberin içerisindeki düşmanlar koşarak oyuncuya saldırmaya çalışsın

            }

        }

        private bool IsAggrevated()
        {
            // düşman ile oyuncu arasındaki mesafe
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            // check aggrevated


            return distanceToPlayer < chaseDistance || timeSinceAggrevated < aggroCooldownTime;
        }


        // düşmanın takip mesafesini görselleştireceğim
        // nesne seçildiğinde çizer
        private void OnDrawGizmosSelected()
        {
            // Gizmos API
            Gizmos.color = Color.blue; // Gizmos mavi renkte olacak
            // küre çizelim
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }




    }

}


