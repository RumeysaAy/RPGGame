/* 
    portallar arası geçişte 
    sahneler arası geçişte
 */

using System.Collections;
using UnityEngine;

namespace RPG.SceneManagement
{
    public class Fader : MonoBehaviour
    {
        CanvasGroup canvasGroup;
        Coroutine currentActiveFade = null;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            // StartCoroutine(FadeOutIn());

        }

        public void FadeOutImmediate()
        {
            canvasGroup.alpha = 1;
            // şeffaf değil
        }

        /*
        
         IEnumerator FadeOutIn()
        {

            yield return FadeOut(3f);
            print("Faded out");
            yield return FadeIn(1f);
            print("Faded In"); // soluklaşır
        }
        */



        // 0 -> 1: belirginleşir
        public Coroutine FadeOut(float time)
        {
            return Fade(1, time);

        }

        // 1 -> 0: soluklaşır
        public Coroutine FadeIn(float time)
        {
            return Fade(0, time);
        }

        /*
        // sınırlı bir süre içinde her karede tekrarlansın
            // alfa 1 olana kadar alfayı güncelleyelim
            while (canvasGroup.alpha > 0)
            {
                // alfa 1 olana kadar ilerler
                canvasGroup.alpha -= Time.deltaTime / time;
                yield return null; // her bir karede çalışır
            }
        */

        public Coroutine Fade(float target, float time)
        {
            if (currentActiveFade != null)
            {
                StopCoroutine(currentActiveFade);
            }
            currentActiveFade = StartCoroutine(FadeRoutine(target, time));
            return currentActiveFade; // sönme
        }

        private IEnumerator FadeRoutine(float target, float time)
        {
            // sınırlı bir süre içinde her karede tekrarlansın
            // target'a eşit olana kadar alfayı güncelleyelim
            while (!Mathf.Approximately(canvasGroup.alpha, target))
            {
                // alfa target'a eşit olana kadar ilerler
                // target'a doğru ilerlemeyi sağlayalım
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.deltaTime / time);
                yield return null; // her bir karede çalışır

            }
        }

    }
}
/*
    Her karede alfayı belirli bir miktarda güncelleyeceğiz
    böylece burada verilen sürenin sonunda alfanın değeri bire ulaşmış olacak
    0'dan 1'e belirginleşir
    (time)/(delta time) = toplam kare miktarı
    1 / toplam kare miktarı = alfa değeri
    0'dan 1'e: 0, 0 + alfa değeri, alfa değeri + alfa değeri, ...
    alfa değeri = 1 * (delta time) / (time)
*/