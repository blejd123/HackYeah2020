using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private float _minLoadingDuration;

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(ChangeSceneInternal(sceneName));
    }

    private IEnumerator ChangeSceneInternal(string sceneName)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        var time = Time.unscaledTime;

        while (!asyncLoad.isDone)
        {
            if (Time.unscaledTime - time >= _minLoadingDuration && asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
