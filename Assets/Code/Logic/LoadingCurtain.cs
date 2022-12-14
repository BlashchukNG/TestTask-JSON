using System.Collections;
using UnityEngine;

namespace Code.Logic
{
    public sealed class LoadingCurtain :
        MonoBehaviour
    {
        [SerializeField] private CanvasGroup _curtain;

        private void Awake() => DontDestroyOnLoad(this);

        public void Show()
        {
            gameObject.SetActive(true);
            _curtain.alpha = 1;
        }

        public void Hide() => StartCoroutine(FadeIn());

        private IEnumerator FadeIn()
        {
            while (_curtain.alpha > 0)
            {
                _curtain.alpha -= 0.03f;
                yield return new WaitForSeconds(0.03f);
            }

            gameObject.SetActive(false);
        }
    }
}