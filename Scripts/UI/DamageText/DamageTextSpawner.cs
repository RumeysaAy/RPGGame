using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.UI.DamageText
{
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] DamageText damageTextPrefab = null;

        public void Spawn(float damageAmount)
        {
            // yeni bir DamageText prefab oluşturmak için DamageText prefab'ını kullanırız
            DamageText instance = Instantiate<DamageText>(damageTextPrefab, transform);
            instance.SetValue(damageAmount);
        }
    }
}
