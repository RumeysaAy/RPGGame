using System;
using GameDevTV.Utils;
using UnityEngine;

/*
xp puanı yükseldikçe level atlanacak
levele göre sağlık değişecek
*/

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 99)]
        [SerializeField] int startingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression = null;
        [SerializeField] GameObject levelUpParticleEffect = null;
        [SerializeField] bool shouldUseModifiers = false;

        public event Action onLevelUp;

        LazyValue<int> currentLevel; // mevcut seviye, şu anki seviye

        Experience experience;

        private void Awake()
        {
            experience = GetComponent<Experience>();
            currentLevel = new LazyValue<int>(CalculateLevel);
        }

        private void Start()
        {
            currentLevel.ForceInit(); // eski level

        }

        private void OnEnable()
        {
            if (experience != null)
            {
                // level, deneyim puanları her değiştiğinde yeniden hesaplanır
                experience.onExperienceGained += UpdateLevel;
            }

        }

        private void OnDisable()
        {
            if (experience != null)
            {
                // level, deneyim puanları her değiştiğinde yeniden hesaplanır
                experience.onExperienceGained -= UpdateLevel;
            }

        }

        private void UpdateLevel()
        {
            // seviye her karede birden fazla kez yeniden hesaplanmaz
            int newLevel = CalculateLevel();
            if (newLevel > currentLevel.value)
            {
                // seviye güncellendi, yükseldi
                currentLevel.value = newLevel;
                // level yükseldiğinde efekt ortaya çıksın ve sağlık artsın
                LevelUpEffect(); // efekt
                onLevelUp(); // sağlık

            }
        }

        private void LevelUpEffect()
        {
            // efekt oluşturulcak, konum
            Instantiate(levelUpParticleEffect, transform);
        }

        public float GetStat(Stat stat)
        {
            // levele göre verilen hasar: GetBaseStat
            /* Silahlarımızın bulunduğumuz seviyeye göre belirlenen hasarın üzerine farklı miktarlarda
            hasar vermesine olanak tanıyan esnek bir değiştirici sistemi oluşturacağız: GetAdditiveModifiers */
            // toplam hasar * ?/100 + toplam hasar = (toplam hasar * (100+?)) / 100
            return (GetBaseStat(stat) + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat) / 100);
        }

        private float GetBaseStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        public int GetLevel()
        {

            return currentLevel.value;
        }

        private float GetAdditiveModifier(Stat stat)
        {
            // düşmanların hasar miktarı artmasın
            // check box'ı işaretli olan sadece oyuncu
            if (!shouldUseModifiers) return 0;

            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifiers(stat))
                {
                    total += modifier;
                    // silah zararı (Weapon damage) + levele göre zarar verme
                    // progression 1. level: 8 + fireball: 5 = 13 toplam verilen hasar

                }
            }
            return total;
        }

        private float GetPercentageModifier(Stat stat)
        {
            // düşmanların hasar miktarı artmasın
            // check box'ı işaretli olan sadece oyuncu
            if (!shouldUseModifiers) return 0;

            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetPercentageModifiers(stat))
                {
                    total += modifier;
                    // silah zararı (Weapon damage) + levele göre zarar verme
                    // progression 1. level: 8 + fireball: 5 = 13 toplam verilen hasar

                }
            }
            return total;

        }

        // deneyim puanlarından seviye hesaplanacak
        private int CalculateLevel()
        {
            Experience experience = GetComponent<Experience>();
            if (experience == null)
            {
                return startingLevel;
            }

            // seviye atlamak için XP puanları: 20, 30, 40, 50
            // 1. seviyedeyken 2. seviyeye geçmek için 20 xp puanı gereklidir.
            // seviye arttıkça sağlık artar
            float currentXP = experience.GetPoints();
            // PenultimateLevel: sondan bir önceki seviye: 4
            // MaxLevel:5
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);
            // PenultimateLevel: 
            for (int level = 1; level <= penultimateLevel; level++)
            {
                float XPToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);
                if (XPToLevelUp > currentXP)
                {
                    return level;
                }
            }
            return penultimateLevel + 1; // bütün seviyelerden geçtiysek
            // Max Level 5

        }



    }

}

// eğer seviye düşükse sağlık hızlı azalır, çünkü sağlık azdır.