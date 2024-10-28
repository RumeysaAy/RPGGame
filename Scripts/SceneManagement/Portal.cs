using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using RPG.Control;

namespace RPG.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        // birden fazla portal var
        // hangi portalın hangi portaldan çıkışı olsun
        enum DestinationIdentifier
        {
            A, B, C, D, E
            // Sandbox ve Sandbox 2: A=Portal, B=Portal (1)

        }


        // Sahneler:
        // Sandbox -> 0
        // Sandbox 2 -> 1
        [SerializeField] int sceneToLoad = -1;
        [SerializeField] Transform spawnPoint;
        [SerializeField] DestinationIdentifier destination; // hedef portal
        [SerializeField] float fadeOutTime = 1f; // soluklaştırmak için
        [SerializeField] float fadeInTime = 2f;
        [SerializeField] float fadeWaitTime = 0.5f;

        private void OnTriggerEnter(Collider other)
        {
            print("Portal Triggered");

            // farklı bir sahneye geçmek için
            if (other.tag == "Player")
            {
                StartCoroutine(Transition());

            }
        }

        private IEnumerator Transition()
        {
            // sahne 0'dan küçükse geçiş olmaz
            if (sceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set.");
                yield break;
            }

            // sahne yüklenmeden önce
            DontDestroyOnLoad(gameObject);
            // saklamak için bir Fader nesnesine sahip olacağım ve nesneyi türe göre bulacağım
            Fader fader = FindObjectOfType<Fader>();
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();

            PlayerController playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            playerController.enabled = false; // remove control

            yield return fader.FadeOut(fadeOutTime); // söner
            // sönme süresi içerisinde sahne yüklenmeden önce oyuncu uzaklaşabilir.

            savingWrapper.Save();
            yield return SceneManager.LoadSceneAsync(sceneToLoad);
            // Save current level - sahneler arası geçiş için

            // remove control
            PlayerController newPlayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            newPlayerController.enabled = false; // remove control


            savingWrapper.Load();


            // oyuncuyu yeniden konumlandırmak için kullanacağımız
            // mevcut portalımızdan yeni sahneye bilgi aktarmamızı sağlar

            // gidilen yerdeki portal
            Portal otherPortal = GetOtherPortal();
            // gidilen yerdeki portalın doğum noktası
            UpdatePlayer(otherPortal); // konumu güncelledikten sonra nav mash agent'ı güncellemeliyiz

            savingWrapper.Save();

            yield return new WaitForSeconds(fadeWaitTime); // bekleme
            // beklerken yeni sahne yüklenir
            // bu süre içerisinde sahne yüklenmeden oyuncu tekrar portala gidebilir
            // farklı bir portala ışınlanabilir bu yüzden oyuncunun elinden
            // kontrolü almamız gerekiyor
            fader.FadeIn(fadeInTime);

            // sahne yüklendikten sonra
            print("Scene Loaded");
            newPlayerController.enabled = true; // Restore control
            // Restore control: oyuncuya kontrolü tekrar geri vereceğiz
            Destroy(gameObject);
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            // oyuncuyu yakalayalım
            GameObject player = GameObject.FindWithTag("Player");

            player.GetComponent<NavMeshAgent>().enabled = false;

            // birden fazla terrain varsa NavMeshAgent hata verir
            //player.GetComponent<NavMeshAgent>().enabled = false;

            // NavMeshAgent'e gitmesini istediğim yerin konumu verelim
            player.GetComponent<NavMeshAgent>().Warp(otherPortal.spawnPoint.position);

            // oyuncunun konumunu güncelleyelim
            // player.transform.position = otherPortal.spawnPoint.position;
            player.transform.rotation = otherPortal.spawnPoint.rotation;

            player.GetComponent<NavMeshAgent>().enabled = true;


            //player.GetComponent<NavMeshAgent>().enabled = true;


        }

        private Portal GetOtherPortal()
        {
            // portal türündeki nesneleri bulmak için
            // portal türündeki nesneleri geri dönderir
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if (portal == this) continue; // bu portal olmasın
                if (portal.destination != destination) continue;

                return portal; // diğer portalı almak için

            }
            return null; // eğer portal yoksa


        }

    }

}

/* karakterimizi bir kapıdan geçtiğinde varsayılan doğma
konumu yerine ilgili portalda olacak şekilde konumlandıracağız

oyuncunun konumunu güncelleme

SpawnPoint, doğma noktasıdır. Doğma noktası portaldan ayrı olmalıdır ve portalın içerisinde olmamalıdır.
böylece doğrudan geri ışınlanmayacağız

Temsilcinin pozisyonu güncellemek istemesi, oyuncunun pozisyonunu güncellememe engel olduğu için bunu çözmenin yolu:
NavMeshAgent’ın Warp (çarpıtma) işlevini kullanmaktır:
player.GetComponent<NavMeshAgent>().Warp()
*/
