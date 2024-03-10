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

    private Mesh[] _meshes;

    private void Start()
    {
        _meshes = new Mesh[_targetText.text.Length];
        for (int i = 0; i < _targetText.text.Length; i++)
        {
            _meshes[i] = CreateMeshAt(i);
        }
        
        CreateEntity();
    }

    private void CreateEntity()
    {
        World world = World.DefaultGameObjectInjectionWorld;
        EntityManager entityManager = world.EntityManager;

        EntityArchetype archetype = entityManager.CreateArchetype(
            typeof(MeshInstanceData),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(RenderBounds));
        
        RenderFilterSettings filterSettings = RenderFilterSettings.Default;
        filterSettings.ShadowCastingMode = ShadowCastingMode.Off;
        filterSettings.ReceiveShadows = false;

        Entity entity = entityManager.CreateEntity(archetype);
        entityManager.SetName(entity, $"MeshEntityTextMesh");
        
        Vector3 randPos = Random.insideUnitSphere;
        entityManager.SetComponentData(entity, new MeshInstanceData
        {
            Position = randPos,
            Rotation = quaternion.identity,
            Scale = Vector3.one,
        });

        GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MeshFilter filter = dummy.GetComponent<MeshFilter>();
        MeshRenderer renderer = dummy.GetComponent<MeshRenderer>();
        
        RenderMesh renderMesh = new RenderMesh
        {
            mesh = filter.mesh,
            material = renderer.material,
            subMesh = 0,
        };
        entityManager.SetSharedComponentManaged(entity, renderMesh);

        Vector3 r = Random.insideUnitSphere;
        entityManager.SetComponentData(entity, new LocalToWorld
        {
            Value = float4x4.TRS(new float3(r.x, r.y, r.z), quaternion.identity, new float3(0.1f)),
        });

        RenderBounds renderBounds = new RenderBounds
        {
            Value = filter.mesh.bounds.ToAABB(),
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

        Glyph glyph = tmpCharacter.glyph;

        float glyphWidth = glyph.metrics.width / fontAsset.faceInfo.pointSize * _fontSize;
        float glyphHeight = glyph.metrics.height / fontAsset.faceInfo.pointSize * _fontSize;

        float x0 = glyph.metrics.horizontalBearingX / fontAsset.faceInfo.pointSize * _fontSize;
        float x1 = x0 + glyphWidth;
        float y0 = -glyph.metrics.horizontalBearingY / fontAsset.faceInfo.pointSize * _fontSize;
        float y1 = y0 - glyphHeight;

        Vector3[] vertices = new[]
        {
            new Vector3(x0, y0, 0),
            new Vector3(x0, y1, 0),
            new Vector3(x1, y1, 0),
            new Vector3(x1, y0, 0),
        };

        Vector2 uv0 = new Vector2(glyph.glyphRect.x / (float)fontAsset.atlasWidth, glyph.glyphRect.y / (float)fontAsset.atlasHeight);
        Vector2 uv1 = new Vector2(uv0.x, (glyph.glyphRect.y + glyph.glyphRect.height) / (float)fontAsset.atlasHeight);
        Vector2 uv2 = new Vector2((glyph.glyphRect.x + glyph.glyphRect.width) / (float)fontAsset.atlasWidth, uv1.y);
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