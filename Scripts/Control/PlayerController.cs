using RPG.Combat;
using RPG.Movement;
using RPG.Attributes;
using UnityEngine;
using System.Data;
using UnityEngine.EventSystems;
using System;
using UnityEngine.AI;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        Health health;

        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] CursorMapping[] cursorMappings = null;
        [SerializeField] float maxNavMeshProjectionDistance = 1f;
        [SerializeField] float raycastRadius = 1f;

        private void Awake()
        {
            health = GetComponent<Health>();
        }


        private void Update()
        {
            if (InteractWithUI())
            {
                // UI ile etkileşime girildiyse
                return;
            }

            if (health.IsDead())
            {
                // ölmüşsek imlecin şekli X olsun
                SetCursor(CursorType.None);
                return;
            }

            if (InteractWithComponent())
            {
                return;

            }

            if (InteractWithMovement())
            {
                // dünya sınırına gidildiğinde hareket etmesin, gitmesin
                return;
            }

            // saldırı veya hareket halinde değilse imleç: None
            SetCursor(CursorType.None);

        }

        private bool InteractWithUI()
        {
            // UI üzerine gelip gelmediğimize bağlı olarak true veya false döndürür
            if (EventSystem.current.IsPointerOverGameObject())
            {
                SetCursor(CursorType.UI);
                return true;
            }
            return false;
        }

        private bool InteractWithComponent()
        {
            RaycastHit[] hits = RaycastAllSorted();
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this))
                    {
                        SetCursor(raycastable.GetCursorType());
                        return true;
                    }
                }
            }
            return false;
        }

        //
        RaycastHit[] RaycastAllSorted()
        {
            /* Pikup’ı yani silahı düşmanın arkasına koyduğum zaman düşmanı görmüyor, 
            silahı görüyor bunu düzeltmemiz gerekli
            sıralama yapacağız */
            // öncelik kameranın önündeki ilk şey

            // 1. Tüm isabetleri alın
            RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), raycastRadius);
            // 2. dizi mesafeleri oluşturun
            float[] distances = new float[hits.Length];

            for (int i = 0; i < hits.Length; i++)
            {
                distances[i] = hits[i].distance;

            }

            // 3. Mesafeye göre sıralayın
            Array.Sort(distances, hits);

            return hits;

            // imleç üzerinde olduğu nesnelerin uzaklıklarına göre seçim yapar
            // önde enemy arkada silah varsa enemy kameraya daha yakın olduğundan imleç enemy'yi görür 

        }

        private bool InteractWithMovement()
        {
            Vector3 target;
            bool hasHit = RaycastNavMesh(out target);
            // hasHit varsa
            if (hasHit)
            {
                if (!GetComponent<Mover>().CanMoveTo(target))
                {
                    return false;
                }

                // farenin sol tuşuna basılı tutulduğunda fareye doğru gelsin
                if (Input.GetMouseButton(0))
                {
                    // hız * 1f
                    GetComponent<Mover>().StartMoveAction(target, 1f);
                    // eğer navigasyon ağında bir noktaya tıklanırsa oraya doğru hareket eder

                    // cismin tıklanılan noktaya gitmesini sağlar
                    // MoveTo(hit.point);
                }
                // hareket anındaki imleç
                SetCursor(CursorType.Movement);
                return true;
            }
            return false;
        }

        /* zemine vurup vurmadığımızı ve bu zemin parçasının bazı navigasyon ağlarına yakın olup olmadığını söylüyoruz,
        böylece esasen yakınlarında navigasyon ağı bulunmayan alanları, buradaki gibi, tepelerin dik yamaçları gibi, eliyoruz. */
        private bool RaycastNavMesh(out Vector3 target)
        {
            // kameranın olduğu yer başlangıç noktası
            // tıkladığımız nokta: Input.mousePosition
            // GetMouseRay()

            target = new Vector3();


            RaycastHit hit;

            // ışın
            // başlangıç noktası, bitiş noktası
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);

            if (!hasHit) return false;

            // en yakındaki navmesh noktası aranır
            NavMeshHit navMeshHit;
            bool hasCastToNavMesh = NavMesh.SamplePosition(
                hit.point, out navMeshHit, maxNavMeshProjectionDistance, NavMesh.AllAreas);
            // en yakında navmesh noktası var mı?
            if (!hasCastToNavMesh) return false; // yoksa


            target = navMeshHit.position;

            ////////////////////////////////////
            /*
            // Dağın tepelerindeki nav mesh’lerde imleç x olsun
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
            if (!hasPath) return false; // yol yoksa
            if (path.status != NavMeshPathStatus.PathComplete) // yol yanlışsa, eksiksiz bir yol mu
            {
                return false; // tam bir yol değilse
                // hareket etmez

                // dağın tepesine çıkmanın bir yolu yok bu yüzden
                // dağın tepesinde navmesh olmasına rağmen
                // imleç dağın tepesinde x olmalı
            }

            if (GetPathLength(path) > maxNavPathLength)
            {

                // yolun uzunluğu çok uzun mu
                // uzunsa
                return false;
            }
            */


            return true; // varsa

            // nav mesh yoksa imleç x olacak
        }



        // imleç türünü ayarlama
        private void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}