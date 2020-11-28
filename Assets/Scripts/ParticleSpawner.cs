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
    private float[] _sizes;
    private float[] _sizesSums;
    private float _totalSize;
    private List<Vector3> _pointNormals;
    private List<Color32> _pointColors;
    private Camera _camera;
    private ParticleSpawnerGlobalSettings _particleSpawnerGlobalSettings;

    private class PointData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color32 Color;

        public PointData(Vector3 position, Vector3 normal, Color32 color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }
    }

    private void Start()
    {
        _particleSpawnerGlobalSettings = FindObjectOfType<ParticleSpawnerGlobalSettings>();
        _camera = Camera.main;
        var meshfilter = GetComponentInChildren<MeshFilter>();
        var mesh = meshfilter.sharedMesh;
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;
        var normals = mesh.normals;
        var colors = mesh.colors32;

        _sizes = GenerateTriangleSizes(vertices, triangles);
        _sizesSums = new float[_sizes.Length];

        _totalSize = 0.0f;
        for (int i = 0; i < _sizesSums.Length; i++)
        {
            _totalSize += _sizes[i];
            _sizesSums[i] = _totalSize;
        }

        var lossyScale = meshfilter.transform.lossyScale;
        var pointCount = (int)(_densityMultiplier * _particleSpawnerGlobalSettings.Density * _totalSize * lossyScale.x * lossyScale.y * lossyScale.z);
        _pointCount = pointCount;
        var m = transform.localToWorldMatrix;
        _points = new List<Vector3>(pointCount);
        _pointNormals = new List<Vector3>(pointCount);
        _pointColors = new List<Color32>(pointCount);
        for (int i = 0; i < pointCount; i++)
        {
            var data = GetRandomPointOnSurface(vertices, triangles, normals, colors);
            _points.Add(m.MultiplyPoint(data.Position));
            _pointNormals.Add(m.MultiplyVector(data.Normal).normalized);
            _pointColors.Add(data.Color);
        }
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
        var waveWidth = _particleSpawnerGlobalSettings.WaveWidth;
        var waveSpeed = _particleSpawnerGlobalSettings.WaveSpeed;
        var distance = range + waveWidth;
        var spawningDuration = distance / waveSpeed;
        var particleSystem = Instantiate(_particleSpawnerGlobalSettings.ParticleSystem);
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

    private PointData GetRandomPointOnSurface(Vector3[] vertices, int[] triangles, Vector3[] normals, Color32[] colors)
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

        var p0 = vertices[triangles[triangleIndex * 3]];
        var p1 = vertices[triangles[triangleIndex * 3 + 1]];
        var p2 = vertices[triangles[triangleIndex * 3 + 2]];
        var a = Random.value;
        var b = Random.value;
        if (a + b >= 1.0f)
        {
            a = 1 - a;
            b = 1 - b;
        }

        var n0 = normals[triangles[triangleIndex * 3]];
        var n1 = normals[triangles[triangleIndex * 3 + 1]];
        var n2 = normals[triangles[triangleIndex * 3 + 2]];

        Color32 color = _colorOverride;
        if (colors != null && colors.Length > 0)
        {
            var c0 = colors[triangles[triangleIndex * 3]];
            var c1 = colors[triangles[triangleIndex * 3 + 1]];
            var c2 = colors[triangles[triangleIndex * 3 + 2]];
            color = new Color32((byte)((c0.r + c1.r + c2.r) / 3), (byte)((c0.g + c1.g + c2.g) / 3), (byte)((c0.b + c1.b + c2.b) / 3), (byte)((c0.a + c1.a + c2.a) / 3));
        }

        var pos = p0 + a * (p1 - p0) + b * (p2 - p0);
        var normal = (n0 + n1 + n2) / 3.0f;
        
        return new PointData(pos, normal, color);
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
