using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI.DamageText
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField] Text damageText = null;

        public void DestroyText()
        {
            // hyerarchy'de birikme olmasın diye
            // tüm prefab'ı yok edelim
            Destroy(gameObject);

        }

        public void SetValue(float amount)
        {
            // vurulan damage'in miktarını gösterir
            damageText.text = String.Format("{0:0}", amount);

        }




    }
}
