using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Fader _fader;
    [SerializeField] private SceneChanger _sceneChanger;

    private void Start()
    {
        _fader.FadeInImmediately();
        StartCoroutine(_fader.FadeOut());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopAllCoroutines();
            Application.Quit();
        }
        else if (Input.anyKeyDown)
        {
            StopAllCoroutines();
            StartCoroutine(GoToGameplay());
        }
    }

    private IEnumerator GoToGameplay()
    {
        yield return _fader.FadeIn();
        _sceneChanger.ChangeScene("Gameplay");
    }
}
