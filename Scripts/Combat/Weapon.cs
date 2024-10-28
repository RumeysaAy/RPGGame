using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] UnityEvent onHit;

        public void OnHit()
        {
            // alınan silaha göre ses efektleri çalışır
            onHit.Invoke();

        }

    }
}
