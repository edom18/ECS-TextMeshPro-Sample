using TMPro;
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
    public float TimeSpeed;
}

public class TmpSpawner : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private TMP_Text _targetText;
    [SerializeField] private int _count = 100;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private float _distributing = 200f;
    [SerializeField] private float _fontSize = 24f;

    private void Start()
    {
        SetupEntities();
    }

    private void SetupEntities()
    {
        for (int c = 0; c < _count; c++)
        {
            for (int i = 0; i < _targetText.text.Length; i++)
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

        float range = _distributing * 0.5f;
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
            TimeSpeed = Random.Range(0.1f, 2f),
        });

        entityManager.AddComponentData(entity, GetCustomUvData(index));
        entityManager.AddComponentData(entity, GetRandomColorData());

        RenderBounds renderBounds = new RenderBounds
        {
            Value = _mesh.bounds.ToAABB(),
        };
        entityManager.SetComponentData(entity, renderBounds);
    }

    private ColorData GetRandomColorData()
    {
        return new ColorData()
        {
            Value = new float4(Random.value, Random.value, Random.value, 1f),
        };
    }

    private CustomUvData GetCustomUvData(int index)
    {
        TMP_FontAsset fontAsset = _targetText.font;
        char character = _targetText.text[index];
        if (!fontAsset.characterLookupTable.TryGetValue(character, out TMP_Character tmpCharacter))
        {
            throw new System.Exception("Character not found in font asset");
        }

        // グリフ情報を取得

        Glyph glyph = tmpCharacter.glyph;

        // グリフのUVを計算

        float rectWidth = glyph.glyphRect.width;
        float rectHeight = glyph.glyphRect.height;
        float atlasWidth = fontAsset.atlasWidth;
        float atlasHeight = fontAsset.atlasHeight;
        float rx = glyph.glyphRect.x;
        float ry = glyph.glyphRect.y;

        float offsetX = rx / atlasWidth;
        float offsetY = ry / atlasHeight;
        float uvScaleX = ((rx + rectWidth) / atlasWidth) - offsetX;
        float uvScaleY = ((ry + rectHeight) / atlasHeight) - offsetY;

        return new CustomUvData()
        {
            Value = new float4(offsetX, offsetY, uvScaleX, uvScaleY),
        };
    }
}