using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Saving;

namespace RPG.SceneManagement
{
    // SavingWrapper portallar arasında geçiş yaparken çağrılır
    public class SavingWrapper : MonoBehaviour
    {
        // nereye kaydedeceğini söyler
        const string defaultSaveFile = "save";
        [SerializeField] float fadeInTime = 0.2f;

        private void Awake()
        {
            StartCoroutine(LoadLastScene());

        }

        private IEnumerator LoadLastScene()
        {
            // kaydedilen son sahne bulunur
            yield return GetComponent<SavingSystem>().LoadLastScene(defaultSaveFile);
            // sahne yüklenir


            /* sadece oyuna ilk başladığımızda gerçekleşir
            Fader'ı kullanarak solar ve opak olur
            */
            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate(); // opak
            yield return fader.FadeIn(fadeInTime); // sönme
        }

        /*
        Saving to /Users/rumeysaay/Library/Application Support/DefaultCompany/RPG Project/save.sav
        Kaydedilmiş bir kayıt dosyasıdı  ve veriler şu anda buraya yerleştirilir
        */

        // Update is called once per frame
        private void Update()
        {
            // L -> yükleme: L tuşuna basıldığında kaydetme sisteminin yükleme yöntemini çağırmak
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();

            }

            if (Input.GetKeyDown(KeyCode.S))
            {

                Save();

            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                Delete(); // kaydedilmiş olan sahne silinir
            }

        }

        public void Save()
        {
            GetComponent<SavingSystem>().Save(defaultSaveFile);
        }

        public void Load()
        {
            // call to saving system load
            GetComponent<SavingSystem>().Load(defaultSaveFile);
        }

        public void Delete()
        {
            GetComponent<SavingSystem>().Delete(defaultSaveFile);
        }


    }
}
