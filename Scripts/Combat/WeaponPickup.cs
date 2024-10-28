using System.Collections;
using RPG.Control;
using UnityEngine;
using RPG.Attributes;


namespace RPG.Combat
{
    public class WeaponPickup : MonoBehaviour, IRaycastable
    {
        [SerializeField] WeaponConfig weapon = null;
        [SerializeField] float healthToRestore = 0; // pumpkin ile sağlık kazanılır
        [SerializeField] float respawnTime = 5;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                Pickup(other.gameObject);
            }
        }

        private void Pickup(GameObject subject)
        {
            if (weapon != null)
            {
                subject.GetComponent<Fighter>().EquipWeapon(weapon);

            }

            if (healthToRestore > 0)
            {
                subject.GetComponent<Health>().Heal(healthToRestore);
            }

            StartCoroutine(HideForSeconds(respawnTime));
        }

        private IEnumerator HideForSeconds(float seconds)
        {
            ShowPickup(false);
            yield return new WaitForSeconds(seconds);
            ShowPickup(true);

        }

        // yerdeki silahların alındıktan bir süre sonra tekrar gösterilmesi için
        private void ShowPickup(bool shouldShow)
        {
            GetComponent<Collider>().enabled = shouldShow;

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(shouldShow);

            }
        }

        // IRaycastable
        public bool HandleRaycast(PlayerController callingController)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Pickup(callingController.gameObject);
                // callingController.GetComponent<Mover>().StartMoveAction(transform.position, 1f);
            }

            return true;
        }

        // IRaycastable
        public CursorType GetCursorType()
        {
            // imleç silahların üzerine geldiğide çanta şeklinde olsun
            return CursorType.Pickup;
        }
    }

}