using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using RPG.Core;
using RPG.Control;

namespace RPG.Cinematics
{
    public class CinematicControlRemover : MonoBehaviour
    {
        GameObject player;

        private void Awake()
        {
            player = GameObject.FindWithTag("Player");
        }

        private void OnEnable()
        {
            GetComponent<PlayableDirector>().played += DisableControl;
            GetComponent<PlayableDirector>().stopped += EnableControl;
        }

        private void OnDisable()
        {
            GetComponent<PlayableDirector>().played -= DisableControl;
            GetComponent<PlayableDirector>().stopped -= EnableControl;
        }

        // oyuncu sinematic izlerken hareket etmemesi için
        void DisableControl(PlayableDirector pd)
        {
            // oyuncu sinematik izlerken kontrol devre dışı bıraklır
            // oyuncunun hareket etmesi ve saldırması engellenir
            print("DisableControl");

            player.GetComponent<ActionScheduler>().CancelCurrentAction();
            player.GetComponent<PlayerController>().enabled = false;
        }

        void EnableControl(PlayableDirector pd)
        {
            // sinematik modun bittiği anlaşılır ve kontrol etkinleştirilir
            print("EnableControl");

            player.GetComponent<PlayerController>().enabled = true;
        }

    }

}