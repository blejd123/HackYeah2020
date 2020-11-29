using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawnerGlobalSettings : MonoBehaviour
{
    public class MeshData
    {
        public Mesh Mesh;
        public float[] Sizes;
        public float[] SizesSums;
        public float TotalSize;
        public Bounds Bounds;
        public Vector3[] Vertices;
        public int[] Triangles;
        public Vector3[] Normals;
        public Color32[] Colors;

        private List<PointData> _randomPoints = new List<PointData>();

        public List<PointData> GetRandomPointsOnSurface(int count, Color32 colorOverride)
        {
            if (_randomPoints.Count < count)
            {
                var toAdd = count - _randomPoints.Count;
                _randomPoints.Capacity = count;
                for (int i = 0; i < toAdd; i++)
                {
                    _randomPoints.Add(GetRandomPointOnSurface(colorOverride));
                }
            }

            if (Colors == null || Colors.Length == 0)
            {
                for (int i = 0; i < count; i++)
                {
                    _randomPoints[i].Color = colorOverride;
                }
            }
            
            return _randomPoints;
        }

        public PointData GetRandomPointOnSurface(Color32 colorOverride)
        {
            int triangleIndex = 0;
            var r = Random.value * TotalSize;
            for (int i = 0; i < SizesSums.Length; i++)
            {
                if (r <= SizesSums[i])
                {
                    triangleIndex = i;
                    break;
                }
            }

            var p0 = Vertices[Triangles[triangleIndex * 3]];
            var p1 = Vertices[Triangles[triangleIndex * 3 + 1]];
            var p2 = Vertices[Triangles[triangleIndex * 3 + 2]];
            var a = Random.value;
            var b = Random.value;
            if (a + b >= 1.0f)
            {
                a = 1 - a;
                b = 1 - b;
            }

            var n0 = Normals[Triangles[triangleIndex * 3]];
            var n1 = Normals[Triangles[triangleIndex * 3 + 1]];
            var n2 = Normals[Triangles[triangleIndex * 3 + 2]];

            Color32 color = colorOverride;
            if (Colors != null && Colors.Length > 0)
            {
                var c0 = Colors[Triangles[triangleIndex * 3]];
                var c1 = Colors[Triangles[triangleIndex * 3 + 1]];
                var c2 = Colors[Triangles[triangleIndex * 3 + 2]];
                color = new Color32((byte)((c0.r + c1.r + c2.r) / 3), (byte)((c0.g + c1.g + c2.g) / 3), (byte)((c0.b + c1.b + c2.b) / 3), (byte)((c0.a + c1.a + c2.a) / 3));
            }

            var pos = p0 + a * (p1 - p0) + b * (p2 - p0);
            var normal = (n0 + n1 + n2) / 3.0f;

            return new PointData(pos, normal, color);
        }
    }

    public class PointData
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

    [SerializeField] private float _density;
    [SerializeField] private ParticleSystem _waveParticleSystem;
    [SerializeField] private AnimationCurve _animationCurve;
    [SerializeField] private ParticleSystem _burstParticleSystem;
    [SerializeField] private float _waveSpeed;
    [SerializeField] private float _waveWidthMultiplier;

    private Dictionary<Mesh, MeshData> _meshDatas = new Dictionary<Mesh, MeshData>();

    public float Density => _density;
    public ParticleSystem WaveParticleSystem => _waveParticleSystem;
    public ParticleSystem BurstParticleSystem => _burstParticleSystem;
    public AnimationCurve AnimationCurve => _animationCurve;
    public float WaveSpeed => _waveSpeed;
    public float WaveWidthMultiplier => _waveWidthMultiplier;

    public MeshData RegisterMesh(Mesh mesh)
    {
        if (_meshDatas.ContainsKey(mesh))
        {
            return _meshDatas[mesh];
        }

        MeshData meshData = new MeshData();
        meshData.Mesh = mesh;

        mesh.RecalculateBounds();
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;
        var normals = mesh.normals;
        var colors = mesh.colors32;

        var sizes = GenerateTriangleSizes(vertices, triangles);
        var sizesSums = new float[sizes.Length];

        var totalSize = 0.0f;
        for (int i = 0; i < sizesSums.Length; i++)
        {
            totalSize += sizes[i];
            sizesSums[i] = totalSize;
        }

        meshData.Sizes = sizes;
        meshData.SizesSums = sizesSums;
        meshData.TotalSize = totalSize;
        meshData.Bounds = mesh.bounds;
        meshData.Vertices = vertices;
        meshData.Triangles = triangles;
        meshData.Normals = normals;
        meshData.Colors = colors;

        _meshDatas.Add(mesh, meshData);
        return meshData;
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
