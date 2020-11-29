using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundAndSpawnParticles : MonoBehaviour
{
    [SerializeField] private float _delay = 5.0f;
    [SerializeField] private bool _randomizeStartDelay = false;
    [SerializeField] private float _duration = 2.0f;
    [SerializeField] private float _normalizedDensity = 1.0f;
    [SerializeField] private ParticleSpawner _particleSpawner;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _clips;

    private void Start()
    {
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;

        if (_randomizeStartDelay)
        {
            yield return new WaitForSeconds(Random.Range(0.0f, _delay));
        }

        while (true)
        {
            yield return new WaitForSeconds(_delay);
            _audioSource.PlayOneShot(_clips[Random.Range(0, _clips.Length)]);
            _particleSpawner.Burst(_duration, _normalizedDensity, null);
        }
    }
}
