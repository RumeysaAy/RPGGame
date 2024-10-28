using RPG.Attributes;
using UnityEngine;
using UnityEngine.Events;

/* mermi */

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 1;
        [SerializeField] bool isHoming = true;
        [SerializeField] GameObject hitEffect = null;
        [SerializeField] float maxLifeTime = 10;
        [SerializeField] GameObject[] destroyOnHit = null;
        [SerializeField] float lifeAfterImpact = 2;
        [SerializeField] UnityEvent onHit;


        Health target = null;
        GameObject instigator = null;
        float damage = 0;

        private void Start()
        {
            transform.LookAt(GetAimLocation()); // ok - mavi

        }


        void Update()
        {
            if (target == null)
            {
                return;
            }

            if (isHoming && !target.IsDead()) // güdümlü ve hedef ölmemişse ise
            {
                transform.LookAt(GetAimLocation()); // güdümlü ok - kırmızı
            }

            transform.Translate(Vector3.forward * speed * Time.deltaTime);

        }

        public void SetTarget(Health target, GameObject instigator, float damage)
        {
            this.target = target;
            this.damage = damage;
            // hasar veren kişi
            this.instigator = instigator;

            Destroy(gameObject, maxLifeTime);


        }

        private Vector3 GetAimLocation()
        {
            CapsuleCollider targetCapsule = target.GetComponent<CapsuleCollider>();
            if (targetCapsule == null)
            {
                return target.transform.position;
            }
            return target.transform.position + Vector3.up * targetCapsule.height / 2;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != target)
            {
                return;
            }

            if (target.IsDead())
            {
                return; // eğer ölmüşse zarar vermeye çalışılmaz
            }

            target.TakeDamage(instigator, damage);
            // mermiyi kim attı? instigator


            speed = 0; // mermi hızı

            // ses
            onHit.Invoke(); // ok ile vurulma sesi


            if (hitEffect != null)
            {
                Instantiate(hitEffect, GetAimLocation(), transform.rotation);
            }

            foreach (GameObject toDestroy in destroyOnHit)
            {
                Destroy(toDestroy);

            }



            Destroy(gameObject, lifeAfterImpact);
        }
    }

}