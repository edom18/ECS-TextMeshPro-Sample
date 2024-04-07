using System;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore;
using Random = UnityEngine.Random;

public struct MeshInstanceData : IComponentData
{
    public float3 Position;
    public quaternion Rotation;
    public float3 Scale;
}

public class TmpSpawner : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private TMP_Text _targetText;
    [SerializeField] private float _fontSize = 24f;
    [SerializeField] private float _unitPerPixel = 100f;
    [SerializeField] private int _count = 100;
    [SerializeField] private Mesh _mesh;

    private Mesh[] _meshes;

    private float FontSizeToUnit => _fontSize / _unitPerPixel;

    private void Start()
    {
        SetupEntities();
    }

    private void SetupEntities()
    {
        _meshes = new Mesh[_targetText.text.Length];
        for (int i = 0; i < _targetText.text.Length; i++)
        {
            _meshes[i] = CreateMeshAt(i);
        }

        for (int c = 0; c < _count; c++)
        {
            for (int i = 0; i < _meshes.Length; i++)
            {
                CreateEntity(i);
            }
        }
    }

    private void CreateEntity(int index)
    {
        World world = World.DefaultGameObjectInjectionWorld;
        EntityManager entityManager = world.EntityManager;

        RenderFilterSettings filterSettings = RenderFilterSettings.Default;
        filterSettings.ShadowCastingMode = ShadowCastingMode.Off;
        filterSettings.ReceiveShadows = false;

        RenderMeshArray renderMeshArray = new RenderMeshArray(new[] { _material }, new[] { _mesh });
        RenderMeshDescription renderMeshDescription = new RenderMeshDescription
        {
            FilterSettings = filterSettings,
            LightProbeUsage = LightProbeUsage.Off,
        };

        Entity entity = entityManager.CreateEntity();
        entityManager.SetName(entity, $"TextMeshEntity {index.ToString()}");

        RenderMeshUtility.AddComponents(
            entity,
            entityManager,
            renderMeshDescription,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

        float range = 100f * 0.5f * FontSizeToUnit;
        float x = Random.Range(-range, range);
        float y = Random.Range(-range, range);
        float z = Random.Range(-range, range);
        float3 position = new float3(x, y, z);
        float3 scale = new float3(1f);

        entityManager.AddComponentData(entity, new MeshInstanceData
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = scale,
        });

        NativeArray<float2> uvs = new NativeArray<float2>(4, Allocator.Temp);
        Mesh mesh = _meshes[index];

        float2 offset = new float2(mesh.uv[0].x, mesh.uv[0].y);
        float uvScaleX = mesh.uv[0].x / mesh.uv[3].x;
        float uvScaleY = mesh.uv[0].y / mesh.uv[3].y;

        entityManager.AddComponentData(entity, new CustomUvData()
        {
            // Value = new float4(offset.x, offset.y, uvScaleX, uvScaleY),
            Value = new float4(0, 0, 0.01f * (index + 1), 0.01f),
        });

        RenderBounds renderBounds = new RenderBounds
        {
            Value = _meshes[index].bounds.ToAABB(),
        };
        entityManager.SetComponentData(entity, renderBounds);
    }

    private Mesh CreateMeshAt(int index)
    {
        TMP_FontAsset fontAsset = _targetText.font;
        char character = _targetText.text[index];
        if (!fontAsset.characterLookupTable.TryGetValue(character, out TMP_Character tmpCharacter))
        {
            throw new System.Exception("Character not found in font asset");
        }

        // グリフ情報を取得

        Glyph glyph = tmpCharacter.glyph;

        // グリフの幅と高さを計算

        float toUnit = 1f / fontAsset.faceInfo.pointSize * FontSizeToUnit;

        float glyphWidth = glyph.metrics.width * toUnit;
        float glyphHeight = glyph.metrics.height * toUnit;

        float x0 = -glyphWidth * 0.5f;
        float x1 = glyphWidth * 0.5f;
        float y0 = -glyphHeight * 0.5f;
        float y1 = glyphHeight * 0.5f;

        Vector3[] vertices = new[]
        {
            new Vector3(x0, y0, 0),
            new Vector3(x0, y1, 0),
            new Vector3(x1, y1, 0),
            new Vector3(x1, y0, 0),
        };

        // グリフのUVを計算

        float rectWidth = glyph.glyphRect.width;
        float rectHeight = glyph.glyphRect.height;
        float atlasWidth = fontAsset.atlasWidth;
        float atlasHeight = fontAsset.atlasHeight;
        float rx = glyph.glyphRect.x;
        float ry = glyph.glyphRect.y;

        Vector2 uv0 = new Vector2(rx / atlasWidth, ry / atlasHeight);
        Vector2 uv1 = new Vector2(uv0.x, (ry + rectHeight) / atlasHeight);
        Vector2 uv2 = new Vector2((rx + rectWidth) / atlasWidth, uv1.y);
        Vector2 uv3 = new Vector2(uv2.x, uv0.y);

        Vector2[] uv = new[]
        {
            uv0, uv1, uv2, uv3,
        };

        int[] triangles = new[]
        {
            0, 1, 2,
            0, 2, 3,
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        return mesh;
    }
}