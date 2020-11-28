using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class ParticleSpawner : MonoBehaviour
{
    [SerializeField] private Color _colorOverride = Color.white;
    [SerializeField] private float _densityMultiplier = 1.0f;

    [SerializeField] private int _pointCount;

    private List<Vector3> _points;
    private List<Vector3> _pointNormals;
    private List<Color32> _pointColors;
    private Camera _camera;
    private ParticleSpawnerGlobalSettings _particleSpawnerGlobalSettings;

    private void Start()
    {
        _particleSpawnerGlobalSettings = FindObjectOfType<ParticleSpawnerGlobalSettings>();
        _camera = Camera.main;
        var meshRenderer = GetComponentInChildren<MeshRenderer>();
        var meshfilter = GetComponentInChildren<MeshFilter>();
        var mesh = meshfilter.sharedMesh;
        var meshData = _particleSpawnerGlobalSettings.RegisterMesh(mesh);

        var b0 = meshData.Bounds.size;
        var b1 = meshRenderer.bounds.size;
        var v0 = Mathf.Max(b0.x, 0.01f) * Mathf.Max(b0.y, 0.01f) * Mathf.Max(b0.z, 0.01f);
        var v1 = Mathf.Max(b1.x, 0.01f) * Mathf.Max(b1.y, 0.01f) * Mathf.Max(b1.z, 0.01f);
        var s = v1 / v0;
        var pointCount = (int)(_densityMultiplier * _particleSpawnerGlobalSettings.Density * meshData.TotalSize * s);
        _pointCount = pointCount;
        var m = transform.localToWorldMatrix;
        Profiler.BeginSample("InitArrays");
        _points = new List<Vector3>(pointCount);
        _pointNormals = new List<Vector3>(pointCount);
        _pointColors = new List<Color32>(pointCount);
        Profiler.EndSample();

        Profiler.BeginSample("GetRandomPoints");
        for (int i = 0; i < pointCount; i++)
        {
            var data = meshData.GetRandomPointOnSurface(_colorOverride);
            _points.Add(m.MultiplyPoint(data.Position));
            _pointNormals.Add(m.MultiplyVector(data.Normal).normalized);
            _pointColors.Add(data.Color);
        }
        Profiler.EndSample();
    }

    public void Burst(float duration, float normalizedDensity, Color32? customColor)
    {
        StartCoroutine(BurstCoroutine(duration, normalizedDensity, customColor));
    }

    public void Wave(Vector3 hitPos, float range)
    {
        StartCoroutine(WaveCoroutine(hitPos, range));
    }

    private IEnumerator WaveCoroutine(Vector3 hitPos, float range)
    {
        Profiler.BeginSample("FindPointsInRange");

        var cameraPos = _camera.transform.position;
        var r2 = range * range;
        var pointsInRange = new List<Vector3>();
        var colorsInRange = new List<Color32>();

        for (int i = 0; i < _points.Count; i++)
        {
            var point = _points[i];
            if ((point - hitPos).sqrMagnitude <= r2)// && Vector3.Dot(_pointNormals[i], cameraPos - point) > 0)
            {
                pointsInRange.Add(point + _pointNormals[i] * 0.01f);
                colorsInRange.Add(_pointColors[i]);
            }
        }
        
        Profiler.EndSample();
        var particleCount = pointsInRange.Count;
        var particles = new ParticleSystem.Particle[particleCount];

        var animationCurve = _particleSpawnerGlobalSettings.AnimationCurve;
        var waveWidth = range * _particleSpawnerGlobalSettings.WaveWidthMultiplier;
        var waveSpeed = _particleSpawnerGlobalSettings.WaveSpeed;
        var distance = range + waveWidth;
        var spawningDuration = distance / waveSpeed;
        var particleSystem = Instantiate(_particleSpawnerGlobalSettings.WaveParticleSystem);
        particleSystem.transform.position = transform.position;
        var main = particleSystem.main;
        main.maxParticles = particleCount;
        main.startLifetime = spawningDuration;

        Profiler.BeginSample("Emit");
        particleSystem.Emit(particleCount);
        Profiler.EndSample();

        Profiler.BeginSample("GetParticles");
        particleSystem.GetParticles(particles);
        Profiler.EndSample();

        Profiler.BeginSample("InitParticles");
        for (int i = 0; i < particleCount; i++)
        {
            particles[i].position = pointsInRange[i];
            //particles[i].startColor = new Color32(0, 255, 0, 0);
            particles[i].startColor = colorsInRange[i];
        }
        Profiler.EndSample();

        Profiler.BeginSample("SetParticles");
        particleSystem.SetParticles(particles, particleCount);
        Profiler.EndSample();
        
        var start = -waveWidth;
        var peakStart = -waveWidth;
        var peakEnd = 0.0f;
        var duration = 0.0f;
        var posOffset = 0.01f;

        while (duration <= spawningDuration)
        {
            peakStart = start + distance * (duration/ spawningDuration);
            peakEnd = peakStart + waveWidth;

            Profiler.BeginSample("UpdateParticles");
            for (int j = 0; j < particleCount; j++)
            {
                var v = pointsInRange[j];
                var dist = (v - hitPos).magnitude;
                var alpha = dist < peakStart || dist > peakEnd ? 0 : Mathf.RoundToInt(animationCurve.Evaluate((dist - peakStart) / waveWidth) * 255);
                var c = colorsInRange[j];
                c.a = (byte) alpha;
                particles[j].startColor = c;
                //particles[j].position = v + _pointNormals[j] * (posOffset + Mathf.Sin(Time.time * 2.0f * (v.x + v.y + v.z)) * posOffset);
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetParticles Inner");
            particleSystem.SetParticles(particles, particleCount);
            Profiler.EndSample();

            duration += Time.deltaTime;
            yield return null;
        }

        Destroy(particleSystem.gameObject);
    }

    private IEnumerator BurstCoroutine(float duration, float normalizedDensity, Color32? customColor)
    {
        var pointCount = Mathf.FloorToInt(_points.Count * Mathf.Clamp01(normalizedDensity));
        var pointsInRange = new List<Vector3>(pointCount);
        var colorsInRange = new List<Color32>(pointCount);
        var posOffset = 0.01f;

        for (int i = 0; i < pointCount; i++)
        {
            var point = _points[i];
            pointsInRange.Add(point + _pointNormals[i] * posOffset);
            colorsInRange.Add(customColor != null ? customColor.Value : _pointColors[i]);
        }

        Profiler.EndSample();
        var particleCount = pointsInRange.Count;
        var particles = new ParticleSystem.Particle[particleCount];
        var particleSystem = Instantiate(_particleSpawnerGlobalSettings.BurstParticleSystem);
        particleSystem.transform.position = transform.position;
        var main = particleSystem.main;
        main.maxParticles = particleCount;
        main.startLifetime = duration;

        Profiler.BeginSample("Emit");
        particleSystem.Emit(particleCount);
        Profiler.EndSample();

        Profiler.BeginSample("GetParticles");
        particleSystem.GetParticles(particles);
        Profiler.EndSample();

        Profiler.BeginSample("InitParticles");
        for (int i = 0; i < particleCount; i++)
        {
            particles[i].position = pointsInRange[i];
            particles[i].startColor = colorsInRange[i];
        }
        Profiler.EndSample();

        Profiler.BeginSample("SetParticles");
        particleSystem.SetParticles(particles, particleCount);
        Profiler.EndSample();

        yield return new WaitForSeconds(duration);
        Destroy(particleSystem.gameObject);
    }
}
