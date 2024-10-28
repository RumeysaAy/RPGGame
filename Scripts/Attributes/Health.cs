using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;
using GameDevTV.Utils;
using UnityEngine.Events;

namespace RPG.Attributes
{
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] float regenerationPercentage = 70;
        [SerializeField] TakeDamageEvent takeDamage;
        // karakterlerin üzerinde aldığı damage'lerin görüntülenmesi için
        [SerializeField] UnityEvent onDie;


        [System.Serializable]
        public class TakeDamageEvent : UnityEvent<float>
        {

        }

        LazyValue<float> healthPoints;
        bool isDead = false;

        private void Awake()
        {
            healthPoints = new LazyValue<float>(GetInitialHealth);
            // sağlık puanını atadım
            // Lazy Value ile kullanımını güvenli hale getirdim
        }

        private float GetInitialHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void Start()
        {
            healthPoints.ForceInit();
        }

        private void OnEnable()
        {
            GetComponent<BaseStats>().onLevelUp += RegenerateHealth;
        }

        private void OnDisable()
        {
            GetComponent<BaseStats>().onLevelUp -= RegenerateHealth;
        }

        public bool IsDead()
        {
            return isDead;
        }

        public void TakeDamage(GameObject instigator, float damage)
        {
            // print(gameObject.name + " took damage" + damage);

            // düşmanın sağlığı en az 0 olacak
            healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);

            if (healthPoints.value == 0)
            {
                onDie.Invoke(); // ses efekti

                Die();
                AwardExperience(instigator);
                // öldüren deneyim puanı kazanır
                // öldüren kişi = instigator
            }
            else
            {
                // hasar aldığımızda karakterin üsttünde alınan hasarın gösterildiği animasyon çalışsın
                // ölmediysek
                takeDamage.Invoke(damage);
            }
        }

        public void Heal(float healthToRestore)
        {
            // pumpkin alan oyuncunun sağlığını artırıyorum
            // max sağlık puanı aşılmasın: min
            healthPoints.value = Mathf.Min(healthPoints.value + healthToRestore, GetMaxHealthPoints());
        }

        public float GetHealthPoints()
        {
            return healthPoints.value;

        }

        public float GetMaxHealthPoints()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);

        }

        public float GetPercentage()
        {
            return 100 * GetFraction();

        }

        // sağlık barını güncellemek için kullanacağım
        public float GetFraction()
        {
            return healthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health);

        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            GetComponent<Animator>().SetTrigger("die");

            // öldüğünde eylem iptal olsun
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void AwardExperience(GameObject instigator)
        {
            Experience experience = instigator.GetComponent<Experience>();
            if (experience == null)
            {
                return;
            }
            experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));

        }

        private void RegenerateHealth()
        {
            // max sağlığın %70'ı alınacak
            float regenHealthPoints = GetComponent<BaseStats>().GetStat(Stat.Health) * (regenerationPercentage / 100);
            // daha fazla sağlık varsa azalmasın
            healthPoints.value = Mathf.Max(healthPoints.value, regenHealthPoints);

        }

        // ######## ISaveable #########
        public object CaptureState()
        {
            return healthPoints.value; // mevcut sağlığımız
        }

        public void RestoreState(object state)
        {
            // sağlık puanlarını daha önce yakalanmış olana ayarlanır.
            healthPoints.value = (float)state;

            if (healthPoints.value == 0)
            {
                Die(); // sağlık sıfırsa ölü görünmeli
            }

        }
        // ############################

    }
}

// iki sahne arasında durum aktarımı sağlık