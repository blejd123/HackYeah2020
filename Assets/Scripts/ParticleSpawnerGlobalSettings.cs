using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawnerGlobalSettings : MonoBehaviour
{
    [SerializeField] private float _density;
    [SerializeField] private ParticleSystem _waveParticleSystem;
    [SerializeField] private AnimationCurve _animationCurve;
    [SerializeField] private ParticleSystem _burstParticleSystem;
    [SerializeField] private float _waveSpeed;
    [SerializeField] private float _waveWidthMultiplier;

    public float Density => _density;
    public ParticleSystem WaveParticleSystem => _waveParticleSystem;
    public ParticleSystem BurstParticleSystem => _burstParticleSystem;
    public AnimationCurve AnimationCurve => _animationCurve;
    public float WaveSpeed => _waveSpeed;
    public float WaveWidthMultiplier => _waveWidthMultiplier;
}
