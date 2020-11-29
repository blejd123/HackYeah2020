﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityController : MonoBehaviour
{
    [SerializeField] private Fader _fader;
    [SerializeField] private SceneChanger _sceneChanger;

    public void EndGame()
    {
        StopAllCoroutines();
        StartCoroutine(GoToMainMenu());
    }

    private void Start()
    {
        _fader.FadeInImmediately();
        StartCoroutine(_fader.FadeOut());
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        StopAllCoroutines();
    //        StartCoroutine(GoToMainMenu());
    //    }
    //}

    private IEnumerator GoToMainMenu()
    {
        yield return _fader.FadeIn();
        _sceneChanger.ChangeScene("MainMenu");
    }
}
