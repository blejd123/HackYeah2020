using System.Collections;
using System.Collections.Generic;
using HackYeah;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayEndTrigger : MonoBehaviour
{
    [SerializeField] private float _delay;
    [SerializeField] private GameplayController _gameplayController;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered)
        {
            return;
        }

        var player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            _triggered = true;
            StartCoroutine(EndGame());
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(_delay);
        _gameplayController.EndGame();
    }
}
