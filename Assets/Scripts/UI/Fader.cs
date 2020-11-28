using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
    [SerializeField] private float _duration;
    [SerializeField] private CanvasGroup _canvasGroup;

    public float Duration => _duration;

    public IEnumerator FadeIn()
    {
        gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = true;

        while (_canvasGroup.alpha < 1.0f)
        {
            _canvasGroup.alpha += (1.0f / _duration) * Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator FadeOut()
    {
        gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = true;

        while (_canvasGroup.alpha > 0.0f)
        {
            _canvasGroup.alpha -= (1.0f / _duration) * Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void FadeInImmediately()
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 1.0f;
        _canvasGroup.blocksRaycasts = true;
    }

    public void FadeOutImmediately()
    {
        gameObject.SetActive(false);
        _canvasGroup.alpha = 0.0f;
        _canvasGroup.blocksRaycasts = false;
    }
}
