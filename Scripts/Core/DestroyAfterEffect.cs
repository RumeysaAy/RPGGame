using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// bittiğinde parçacık sistemini temizlemek için
// efekt sonrası yok et
namespace RPG.Core
{
    public class DestroyAfterEffect : MonoBehaviour
    {
        [SerializeField] GameObject targetToDestory = null;

        void Update()
        {
            // mevcut parçacık sistemi üzerinde çalışıyor
            if (!GetComponent<ParticleSystem>().IsAlive())
            {
                if (targetToDestory != null)
                {
                    // varsa hedefi yok eder
                    Destroy(targetToDestory);
                    // hedefe ebeveyn konulur ve parçacık sistemi bittiğinde ebeveyni öldürür
                    // tüm prefabrik yok olur

                }
                else
                {
                    // üzerinde bulunduğu oyun nesnesini yok eder
                    Destroy(gameObject); // mevcut oyun nesnesini yok ediyor
                                         // level arttığında ebeveyni de yok etmeliyim
                }

            }

        }
    }
}