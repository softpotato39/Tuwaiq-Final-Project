using UnityEngine;
using System.Collections;

namespace InteractionSystem
{
    public class AnomalyHighlight : MonoBehaviour
    {
        [SerializeField] private GameObject highlightVisual;    // child GameObject with an emissive or outline material MAKE IT DISABLED :)

        private Coroutine _hideRoutine;

        public void ShowHighlight(float duration)
        {
            // if its already highlighted, restart the timer
            if (_hideRoutine != null)
                StopCoroutine(_hideRoutine);

            highlightVisual?.SetActive(true);
            _hideRoutine = StartCoroutine(HideAfter(duration));
        }

        private IEnumerator HideAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            highlightVisual?.SetActive(false);
        }
    }
}