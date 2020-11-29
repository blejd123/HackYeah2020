using System.Collections;
using System.Collections.Generic;
using HackYeah;
using UnityEngine;

public class CityEndTrigger : MonoBehaviour
{
    [SerializeField] private float _delay;
    [SerializeField] private CityController _cityController;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered)
        {
            return;
        }

        var camera = other.GetComponentInParent<Camera>();
        if (camera != null)
        {
            _triggered = true;
            StartCoroutine(EndGame());
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(_delay);
        _cityController.EndGame();
    }
}
