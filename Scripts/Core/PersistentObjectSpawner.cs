using System;
using UnityEngine;

namespace RPG.Core
{
    public class PersistentObjectSpawner : MonoBehaviour
    {
        [SerializeField] GameObject persistentObjectPrefab;

        static bool hasSpawned = false;


        private void Awake()
        {
            if (hasSpawned)
            {
                return;
            }

            SpawnPersistentObjects();

            hasSpawned = true;

        }

        private void SpawnPersistentObjects()
        {
            // prefabriği oluşturacağım ve yüklendiğinde yok edilmeyecek şekilde ayarlamaktadır.
            // soluklaştırma işlemi
            GameObject persistentObject = Instantiate(persistentObjectPrefab);
            // yok edilmesin diye
            DontDestroyOnLoad(persistentObject);




        }

    }
}