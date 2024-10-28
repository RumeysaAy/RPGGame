using RPG.Attributes;
using RPG.Core;
using UnityEngine;
using UnityEngine.AI;
using RPG.Saving;

namespace RPG.Movement
{

    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        [SerializeField] Transform target;
        [SerializeField] float maxSpeed = 6f;
        // düşmanın devriye gezmediği sıradaki hızı
        [SerializeField] float maxNavPathLength = 40f;


        NavMeshAgent navMeshAgent;
        Health health;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
        }

        void Update()
        {
            // öldüğünde nav mesh'i devre dışı bıraktım
            // çünkü oyuncu düşmanın üzerinden geçemiyordu

            // oyuncu ölür ölmez düşmanın navMeshAgent bileşeni devre dışı bırakılacak
            navMeshAgent.enabled = !health.IsDead();


            UpdateAnimator();

            // başlangıç noktasından tıkladığımız noktaya ışın
            // Debug.DrawRay(lastRay.origin, lastRay.direction * 100);
        }

        // düşmandan uzaklaşması için
        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(destination, speedFraction);

        }

        public bool CanMoveTo(Vector3 destination)
        {

            // Dağın tepelerindeki nav mesh’lerde imleç x olsun
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
            if (!hasPath) return false; // yol yoksa
            if (path.status != NavMeshPathStatus.PathComplete) // yol yanlışsa, eksiksiz bir yol mu
            {
                return false; // tam bir yol değilse
                // hareket etmez

                // dağın tepesine çıkmanın bir yolu yok bu yüzden
                // dağın tepesinde navmesh olmasına rağmen
                // imleç dağın tepesinde x olmalı
            }

            if (GetPathLength(path) > maxNavPathLength)
            {

                // yolun uzunluğu çok uzun mu
                // uzunsa
                return false;
            }

            return true;


        }


        public void MoveTo(Vector3 destination, float speedFraction)
        {
            // cismin noktaya hareket etmesini sağlar
            navMeshAgent.destination = destination;
            // değerin 0 ile 1 arasında olması gerektiğini söyler
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        public void Cancel()
        {
            navMeshAgent.isStopped = true;

        }


        // yürüme animasyonlarını kullanabilmek için yerel hıza dönüştürülür
        private void UpdateAnimator()
        {
            Vector3 velocity = navMeshAgent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity); // yerel hız

            float speed = localVelocity.z;
            GetComponent<Animator>().SetFloat("forwardSpeed", speed); // animasyon hız

        }

        /*
        [System.Serializable]
        struct MoverSaveData
        {
            public SerializableVector3 position;
            public SerializableVector3 rotation;
        }
        */

        private float GetPathLength(NavMeshPath path)
        {
            /* ol boyunca tüm bu noktaların bir listesine sahipsiniz.
            Yapmanız gereken şey, bu noktaların her biri arasındaki mesafeyi bulmak ve sonra bunları
            toplamak ve kendinize yol uzunluğunu elde etmektir. */
            float total = 0;

            // tüm bu farklı köşelerin toplam uzunluğunu hesaplaplanması
            if (path.corners.Length < 2) // ikiden az köşesi olan yollar iptal edilir çünkü mesafe hesaplanamaz
            {
                return total;
            }
            // köşeler arasındaki tüm bitler üzerinde dört döngü yapmak isteyebiliriz
            // dizide her zaman iki köşeyi karşılaştıracağız
            // aralarındaki mesafeyi bulmak için dizideki öğeleri aynı anda karşılaştırabiliriz
            for (int i = 0; i < (path.corners.Length - 1); i++) // boşluk, -1 kadardır
            {
                // mesafeyi hesaplayalım
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return total; // mesafe > 40 ise hareket etmez, gitmeye çalışmaz
        }





        // ######### ISaveable #######
        public object CaptureState()
        {
            return new SerializableVector3(transform.position);

            /*
            MoverSaveData data = new MoverSaveData();
            data.position = new SerializableVector3(transform.position);
            data.rotation = new SerializableVector3(transform.eulerAngles);
            return data;
            */


            /*
            //Herhangi bir bileşen için birden fazla veriyi kaydetmek
            // durumu yakalayan bir sözcük
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["position"] = new SerializableVector3(transform.position); // mevcut konum aktarılır
            data["rotation"] = new SerializableVector3(transform.eulerAngles);
            return data;
            */

        }

        // Saving to /Users/rumeysaay/Library/Application Support/DefaultCompany/RPG Project/save.sav
        // dünyada sahip olduğumuz tüm pozisyonları kayıt dosyamıza kaydetti

        public void RestoreState(object state)
        {

            SerializableVector3 position = (SerializableVector3)state;
            navMeshAgent.enabled = false;
            transform.position = position.ToVector();
            navMeshAgent.enabled = true;

            //
            GetComponent<ActionScheduler>().CancelCurrentAction();

            /*
            MoverSaveData data = (MoverSaveData)state;
            GetComponent<NavMeshAgent>().enabled = false;
            transform.position = data.position.ToVector();
            transform.eulerAngles = data.rotation.ToVector();
            GetComponent<NavMeshAgent>().enabled = true;
            */

            /*
            // bir sonraki adım bu pozisyonları eski haline getirmektir
            Dictionary<string, object> data = (Dictionary<string, object>)state; // state, Dictionary<string, object> türündedir.
            GetComponent<NavMeshAgent>().enabled = false; // NavMeshAgent hata verdiği için devre dışı bıraktım
            transform.position = ((SerializableVector3)data["position"]).ToVector(); // bulunduğu yere geri getirdim
            transform.eulerAngles = ((SerializableVector3)data["rotation"]).ToVector();
            GetComponent<NavMeshAgent>().enabled = true;
            */

        }
    }

}

// S ile konumu kaydederiz
// L ile eski yani kaydettiğimiz konuma geri döneriz