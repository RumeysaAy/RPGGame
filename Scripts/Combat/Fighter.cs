using RPG.Core;
using RPG.Movement;
using RPG.Saving;
using UnityEngine;
using RPG.Attributes;
using RPG.Stats;
using System.Collections.Generic;
using GameDevTV.Utils;


namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {
        [SerializeField] float timeBetweenAttacks = 1f;

        [SerializeField] Transform rightHandTransform = null; // silahsızsa
        [SerializeField] Transform leftHandTransform = null; // silahsızsa

        [SerializeField] WeaponConfig defaultWeapon = null;


        Health target;
        // oyuncunun saldırıya geçmesi çok uzun sürüyor
        // Oyuncunun saldırı süresini kısaltalım
        float timeSinceLastAttack = Mathf.Infinity;
        // başlangıçta son saldırıdan bu yana geçen süre sıfırdır
        // bu, oyuna başladığımızda sanki bir saldırı yapmışız gibi etkili olacağı anlamına geliyor
        // sanki çok uzun zaman önce bir saldırı yapmışız gibi olmasını istiyoruz

        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;

        private void Awake()
        {
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);
        }

        private Weapon SetupDefaultWeapon()
        {
            return AttachWeapon(defaultWeapon);
        }


        public void Start()
        {
            currentWeapon.ForceInit();
        }


        // düşmana belirli bir mesafeden saldırmak için
        // düşmana doğru gidilsin ama saldırı menzilinde dursun
        private void Update()
        {
            // menzilin içerisinde miyiz?
            // bool isInRange = GetIsInRange();

            timeSinceLastAttack += Time.deltaTime;

            if (target == null) return;

            if (target.IsDead())
            {
                return;
            }


            if (!GetIsInRange(target.transform))
            {
                // düşmandan belirli bir mesafede durmak için
                GetComponent<Mover>().MoveTo(target.transform.position, 1f); // hız=hız*1f
            }
            else
            {
                // menzilin dışında kalsın, orada dursun
                GetComponent<Mover>().Cancel();

                // saldırıya başlar
                AttackBehaviour();
            }
        }


        public void EquipWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            // if (weapon == null) return;
            currentWeapon.value = AttachWeapon(weapon);

        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            Animator animator = GetComponent<Animator>();
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);

        }

        public Health GetTarget()
        {
            return target;

        }

        private void AttackBehaviour()
        {
            // saldırırken düşmana baksın
            transform.LookAt(target.transform);

            if (timeSinceLastAttack > timeBetweenAttacks)
            {
                TriggerAttack();
                timeSinceLastAttack = 0;

            }

        }

        private void TriggerAttack()
        {
            GetComponent<Animator>().ResetTrigger("stopAttack");
            // bu, Hit() olayını tetikleyecektir.
            GetComponent<Animator>().SetTrigger("attack");

        }

        // Animation Event
        void Hit()
        {
            if (target == null)
            {
                return;
            }

            /*
            Silahlarımızın bulunduğumuz seviyeye göre belirlenen hasarın üzerine farklı 
            miktarlarda hasar vermesine olanak tanıyan esnek bir değiştirici sistemi 
            oluşturacağız
            */

            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);

            if (currentWeapon.value != null)
            {
                currentWeapon.value.OnHit();

            }

            if (currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(rightHandTransform, leftHandTransform, target, gameObject, damage);
            }
            else
            {
                // düşmanın sağlığını azaltmak için
                target.TakeDamage(gameObject, damage);
                // hedefe zarar veren kişi = instigator = gameObject

                /*
                 * Düşmanların ve oyuncuların verdiği hasarın seviyelerine bağlı olarak değişmesini sağlanacak
                 * Böylece seviye atladığımızda bir asker ile karşılaşırsak onlara saldırmak daha kolay olur.
                 * Seviye atladıkça hasarı artıracağız
                 * damage = GetComponent<BaseStats>().GetStat(Stat.Damage)
                 */
            }


        }

        void Shoot()
        {
            Hit();

        }

        private bool GetIsInRange(Transform targetTransform)
        {
            return Vector3.Distance(transform.position, targetTransform.position) < currentWeaponConfig.GetRange();
        }

        /*
            düşman oyuncuya saldırır,
            oyuncu tıklanılan düşmana saldırır
            */

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null) { return false; }
            if (!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position) &&
                !GetIsInRange(combatTarget.transform))
            {
                // düşmana gidecek yol olmasa bile düşmana saldırabilmek için
                // ok ve fireball ile
                // !GetIsInRange(combatTarget.transform)
                return false;
            }
            // ölen düşmanın arkasında kalan düşmanı öldürmek için
            Health targetToTest = combatTarget.GetComponent<Health>();
            // karakter ölmüşse false dönderir
            return targetToTest != null && !targetToTest.IsDead();




        }


        public void Attack(GameObject combatTarget)
        {

            GetComponent<ActionScheduler>().StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            StopAttack();
            target = null;
            GetComponent<Mover>().Cancel();
        }

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");

        }

        // ############# IModifierProvider #################
        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetDamage();
            }

        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetPercentageBonus();
            }
        }

        /*
        /Users/rumeysaay/UnityProjeleri/RPG Project/Assets/Game/Weapons/Resources
        Sword yani kılıçla öldürürsek yüzde bonus 10/100

        Sword damage: 10 
        Base stat: 8 (level 1 için)
        Bonus: %10
        18*(10/100) + 18 = toplam hasar = 19.8
        */
        // ################## IModifierProvider ################


        // ############## ISaveable ##############
        public object CaptureState()
        {
            // aynı anda sadece bir tane silah kullanılır
            return currentWeaponConfig.name;

        }

        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
            EquipWeapon(weapon);
        }
        // ############### ISaveable ################




    }
}

