using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPG.Control
{
    public class PatrolPath : MonoBehaviour
    {
        const float waypointGizmoRadius = 0.3f;


        // düşmanların yürüyeceği yolu görselleştirdim
        private void OnDrawGizmos()
        {
            // noktalar ve bu noktalar arasında geçiş
            // child: Waypoint, Waypoint 1, Waypoint 2...
            for (int i = 0; i < transform.childCount; i++)
            {
                int j = GetNextIndex(i);
                // kürenin nerede olacağı, yarıçapı
                Gizmos.DrawSphere(GetWaypoint(i), waypointGizmoRadius); // noktalar görselleştirildi

                // yolu görselleştirelim
                // başlangıç, bitiş noktası
                Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(j));
            }

        }

        // mevcut yol noktasını al ve yol noktasını değiştir

        // indeks verilir ve bir sonraki indeks geri dönderilir
        public int GetNextIndex(int i)
        {
            // eğer son noktaya geldiyse birinci noktaya gitsin
            if ((i + 1) == transform.childCount)
            {
                return 0;
            }

            return i + 1;
        }

        // indeks verilir, konum geri dönderilir
        public Vector3 GetWaypoint(int i)
        {
            return transform.GetChild(i).position;
        }



    }

}
