using UnityEngine;
using RPG.Attributes;
using RPG.Control;

namespace RPG.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        // IRaycastable
        public CursorType GetCursorType()
        {
            return CursorType.Combat;
        }

        // IRaycastable
        public bool HandleRaycast(PlayerController callingController)
        {
            if (!callingController.GetComponent<Fighter>().CanAttack(gameObject))
            {
                return false;
                // saldıramazsa ölmüş olabiliriz
            }

            if (Input.GetMouseButton(0))
            {
                // oyuncuya saldırır
                callingController.GetComponent<Fighter>().Attack(gameObject);
            }

            return true; // düşmanın üzerine tıklayınca hareket etmesin
        }
    }

}