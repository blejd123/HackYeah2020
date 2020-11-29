using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnParticles : MonoBehaviour
{
    [SerializeField] private float _delay = 5.0f;
    [SerializeField] private bool _randomizeStartDelay = false;
    [SerializeField] private float _randomDelay = 2.0f;
    [SerializeField] private float _duration = 2.0f;
    [SerializeField] private float _normalizedDensity = 1.0f;
    [SerializeField] private ParticleSpawner _particleSpawner;

    private void Start()
    {
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        if (_randomizeStartDelay)
        {
            yield return new WaitForSeconds(Random.Range(0.0f, _delay));
        }

        while (true)
        {
            yield return new WaitForSeconds(_delay + Random.Range(0.0f, _randomDelay));
            _particleSpawner.Burst(_duration, _normalizedDensity, null);
        }
    }
}
