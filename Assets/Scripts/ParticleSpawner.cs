using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class ParticleSpawner : MonoBehaviour
{
    [SerializeField] private int _pointCount;
    [SerializeField] private ParticleSystem _particleSystem;
    //[SerializeField] private int _particleCount;
    [SerializeField] private Color _color;
    [SerializeField] private AnimationCurve _animationCurve;

    private List<Vector3> _points;
    //private ParticleSystem.Particle[] _particles;
    private float[] _sizes;
    private float[] _sizesSums;
    private float _totalSize;

    private void Start()
    {
        var meshRenderer = GetComponentInChildren<MeshRenderer>();
        var meshfilter = GetComponentInChildren<MeshFilter>();
        var mesh = meshfilter.sharedMesh;
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        _sizes = GenerateTriangleSizes(vertices, triangles);
        _sizesSums = new float[_sizes.Length];

        _totalSize = 0.0f;
        for (int i = 0; i < _sizesSums.Length; i++)
        {
            _totalSize += _sizes[i];
            _sizesSums[i] = _totalSize;
        }

        var m = transform.localToWorldMatrix;
        _points = new List<Vector3>(_pointCount);
        for (int i = 0; i < _pointCount; i++)
        {
            _points.Add(m.MultiplyPoint(GetRandomPointOnSurface(vertices, triangles)));
        }

        //_particles = new ParticleSystem.Particle[_particleCount];
        //var main = _particleSystem.main;
        //main.maxParticles = _particleCount;
        //_particleSystem.Emit(_particleCount);
        //_particleSystem.GetParticles(_particles);
        //for (int i = 0; i < _particleCount; i++)
        //{
        //    _particles[i].position = _points[Random.Range(0, _pointCount)];
        //    //_particles[i].startColor = new Color32(0, 255, 0, 55);
        //}
        //_particleSystem.SetParticles(_particles, _particleCount);
    }

    private void Update()
    {
        
    }

    public void Wave(Vector3 hitPos, float range)
    {
        StartCoroutine(WaveCoroutine(hitPos, range));
    }

    private IEnumerator WaveCoroutine(Vector3 hitPos, float range)
    {
        Profiler.BeginSample("FindPointsInRange");
        var pointsInRange = _points.FindAll(x => (x - hitPos).magnitude <= range);
        Profiler.EndSample();
        var particleCount = pointsInRange.Count;
        var particles = new ParticleSystem.Particle[particleCount];

        var spawningDuration = 3.0f;
        var main = _particleSystem.main;
        main.maxParticles = particleCount;
        main.startLifetime = spawningDuration;
        var frames = Mathf.FloorToInt(spawningDuration / Time.fixedDeltaTime);

        Profiler.BeginSample("Emit");
        _particleSystem.Emit(particleCount);
        Profiler.EndSample();

        Profiler.BeginSample("GetParticles");
        _particleSystem.GetParticles(particles);
        Profiler.EndSample();

        Profiler.BeginSample("InitParticles");
        for (int i = 0; i < particleCount; i++)
        {
            particles[i].position = pointsInRange[i];
            particles[i].startColor = new Color32(0, 255, 0, 0);
        }
        Profiler.EndSample();

        Profiler.BeginSample("SetParticles");
        _particleSystem.SetParticles(particles, particleCount);
        Profiler.EndSample();

        var peakWidth = 1.5f;
        var start = -peakWidth;
        var peakStart = -peakWidth;
        var peakEnd = 0.0f;
        var distance = range + peakWidth;
        var duration = 0.0f;

        while (duration <= spawningDuration)
        {
            peakStart = start + distance * (duration/ spawningDuration);
            peakEnd = peakStart + peakWidth;

            Profiler.BeginSample("UpdateParticles");
            for (int j = 0; j < particleCount; j++)
            {
                var v = pointsInRange[j];
                var dist = (v - hitPos).magnitude;
                var alpha = dist < peakStart || dist > peakEnd ? 0 : Mathf.RoundToInt(_animationCurve.Evaluate((dist - peakStart) / peakWidth) * 255);
                particles[j].startColor = new Color32(0, 255, 0, (byte)alpha);
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetParticles Inner");
            _particleSystem.SetParticles(particles, particleCount);
            Profiler.EndSample();

            duration += Time.deltaTime;
            yield return null;
        }
    }

    private Vector3 GetRandomPointOnSurface(Vector3[] vertexes, int[] triangles)
    {
        int triangleIndex = 0;
        var r = Random.value * _totalSize;
        for (int i = 0; i < _sizesSums.Length; i++)
        {
            if (r <= _sizesSums[i])
            {
                triangleIndex = i;
                break;
            }
        }

        var p0 = vertexes[triangles[triangleIndex * 3]];
        var p1 = vertexes[triangles[triangleIndex * 3 + 1]];
        var p2 = vertexes[triangles[triangleIndex * 3 + 2]];
        var a = Random.value;
        var b = Random.value;
        if (a + b >= 1.0f)
        {
            a = 1 - a;
            b = 1 - b;
        }

        return p0 + a * (p1 - p0) + b * (p2 - p0);
    }

    private float[] GenerateTriangleSizes(Vector3[] vertexes, int[] triangles)
    {
        var triangleCount = triangles.Length / 3;
        var sizes = new float[triangleCount];

        for (int i = 0; i < triangleCount; i++)
        {
            var p0 = vertexes[triangles[i * 3]];
            var p1 = vertexes[triangles[i * 3 + 1]];
            var p2 = vertexes[triangles[i * 3 + 2]];
            var a = (p1 - p0).magnitude;
            var b = (p2 - p1).magnitude;
            var c = (p2 - p0).magnitude;
            var p = 0.5f * (a + b + c);
            sizes[i] = Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        return sizes;
    }
}
