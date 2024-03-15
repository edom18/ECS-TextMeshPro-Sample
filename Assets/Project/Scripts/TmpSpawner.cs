using System;
using TMPro;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore;
using UnityEngine.UIElements;
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

    private Mesh[] _meshes;

    private float FontSizeToUnit => _fontSize / _unitPerPixel;

    private void Start()
    {
        _meshes = new Mesh[_targetText.text.Length];
        for (int i = 0; i < _targetText.text.Length; i++)
        {
            _meshes[i] = CreateMeshAt(i);
        }

        for (int i = 0; i < _meshes.Length; i++)
        {
            CreateEntity(i);
        }
    }

    private void CreateEntity(int index)
    {
        World world = World.DefaultGameObjectInjectionWorld;
        EntityManager entityManager = world.EntityManager;

        RenderFilterSettings filterSettings = RenderFilterSettings.Default;
        filterSettings.ShadowCastingMode = ShadowCastingMode.Off;
        filterSettings.ReceiveShadows = false;

        RenderMeshArray renderMeshArray = new RenderMeshArray(new[] { _material }, _meshes);
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
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, index));

        float x = index * FontSizeToUnit;
        float3 position = new float3(x, 0, 0);
        float3 scale = new float3(1f);
        entityManager.SetComponentData(entity, new LocalToWorld()
        {
            Value = float4x4.TRS(
                position,
                quaternion.identity,
                scale),
        });
        
        entityManager.AddComponentData(entity, new MeshInstanceData
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = scale,
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

        Vector3[] vertices = new[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, glyphHeight, 0),
            new Vector3(glyphWidth, glyphHeight, 0),
            new Vector3(glyphWidth, 0, 0),
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