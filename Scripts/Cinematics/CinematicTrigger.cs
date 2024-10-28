using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    public class CinematicTrigger : MonoBehaviour
    {
        bool alreadyTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            // tetikleyenin etiketi "Player" mı? 
            if (!alreadyTriggered && other.gameObject.tag == "Player")
            {
                // sadece oyuncu ilk tetiklediğinde çalışsın
                alreadyTriggered = true;
                GetComponent<PlayableDirector>().Play();
            }


        }

    }
}
