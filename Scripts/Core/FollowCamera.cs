using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class FollowCamera : MonoBehaviour
    {

        // kameranın oyuncuyu takip etmesi
        [SerializeField] Transform target;

        void LateUpdate() // kamera karakterden sonra gelsin
        {
            transform.position = target.position;

        }
    }

}
