using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    [SerializeField] private Fader _fader;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _continue;
    [SerializeField] private Button _exit;
    [SerializeField] private GameplayController _gameplayController;
    [SerializeField] private CityController _cityController;

    private void Start()
    {
        _continue.onClick.AddListener(OnContinueClick);
        _exit.onClick.AddListener(OnExitClick);
        _panel.SetActive(false);
    }

    private void OnDestroy()
    {
        _continue.onClick.RemoveListener(OnContinueClick);
        _exit.onClick.RemoveListener(OnExitClick);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !_panel.activeSelf)
        {
            StopAllCoroutines();
            StartCoroutine(Pause());
        }
    }

    private IEnumerator Pause()
    {
        yield return null;
        if (_gameplayController != null)
        {
            _gameplayController.ExitGame();
        }
        else
        {
            //yield return _fader.FadeIn();
            _panel.SetActive(true);
            Time.timeScale = 0.0f;
        }
    }

    private IEnumerator Unpause()
    {
        Time.timeScale = 1.0f;
        _panel.SetActive(false);
        yield return null;
        //yield return _fader.FadeOut();
    }

    private void OnContinueClick()
    {
        StopAllCoroutines();
        StartCoroutine(Unpause());
    }

    private void OnExitClick()
    {
        if (_gameplayController != null)
        {
            Time.timeScale = 1.0f;
            _gameplayController.EndGame();
        }
        else if (_cityController != null)
        {
            Time.timeScale = 1.0f;
            _cityController.EndGame();
        }
    }
}

