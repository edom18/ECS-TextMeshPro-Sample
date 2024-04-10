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
    public float MoveSpan;
}

public class TmpSpawner : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private TMP_Text _targetText;
    [SerializeField] private int _count = 100;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private float _distributing = 200f;
    [SerializeField] private Vector2 _moveSpanRange = new Vector2(0.1f, 0.5f);

    [SerializeField, Tooltip("フォントサイズをcmで指定")]
    private float _fontSizeInCm = 24f;

    // フォントサイズの単位はcmとする
    private float FontSizeToUnit => _fontSizeInCm * 0.01f;

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
        
        Entity entity = entityManager.CreateEntity();
        entityManager.SetName(entity, $"TextMeshEntity {index.ToString()}");

        RenderFilterSettings filterSettings = RenderFilterSettings.Default;
        filterSettings.ShadowCastingMode = ShadowCastingMode.Off;
        filterSettings.ReceiveShadows = false;

        RenderMeshArray renderMeshArray = new RenderMeshArray(new[] { _material }, new[] { _mesh });
        RenderMeshDescription renderMeshDescription = new RenderMeshDescription
        {
            FilterSettings = filterSettings,
            LightProbeUsage = LightProbeUsage.Off,
        };

        RenderMeshUtility.AddComponents(
            entity,
            entityManager,
            renderMeshDescription,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

        Vector3 p = Random.insideUnitSphere * _distributing;
        float3 position = new float3(p.x, p.y, p.z);
        quaternion rotation = quaternion.identity;
        float3 scale = GetScale(index);
        float timeSpeed = Random.Range(0.1f, 2f);
        float moveSpan = Random.Range(_moveSpanRange.x, _moveSpanRange.y);
        
        entityManager.AddComponentData(entity, new MeshInstanceData
        {
            Position = position,
            Rotation = rotation,
            Scale = scale,
            TimeSpeed = timeSpeed,
            MoveSpan = moveSpan,
        });

        entityManager.AddComponentData(entity, GetCustomUvData(index));
        entityManager.AddComponentData(entity, GetRandomColorData());
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

    private float3 GetScale(int index)
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

        return new float3(glyphWidth, glyphHeight, 1f);
    }
}