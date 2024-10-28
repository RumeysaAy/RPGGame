using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Attributes
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Health healthComponent = null;
        [SerializeField] RectTransform foreground = null;
        [SerializeField] Canvas rootCanvas = null;
        void Update()
        {
            // sağlık oranı yaklaşık olarak 0'a veya 1'e eşitse
            if (Mathf.Approximately(healthComponent.GetFraction(), 0)
            || Mathf.Approximately(healthComponent.GetFraction(), 1))
            {
                rootCanvas.enabled = false;
                return;

            }
            rootCanvas.enabled = true;


            foreground.localScale = new Vector3(healthComponent.GetFraction(), 1, 1);

            // sağlık bileşeninden sağlık değerini alalım




        }
    }
}

// sağlık tamamen doluysa çubuklar gözükmesin
// saldırmaya başladığında gözüksün
// sağlık 0 olduğunda çubuk kaybolsun
