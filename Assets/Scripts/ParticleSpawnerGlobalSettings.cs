using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawnerGlobalSettings : MonoBehaviour
{
    [SerializeField] private float _density;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private AnimationCurve _animationCurve;
    [SerializeField] private float _waveSpeed;
    [SerializeField] private float _waveWidth;

    public float Density => _density;
    public ParticleSystem ParticleSystem => _particleSystem;
    public AnimationCurve AnimationCurve => _animationCurve;
    public float WaveSpeed => _waveSpeed;
    public float WaveWidth => _waveWidth;
}
