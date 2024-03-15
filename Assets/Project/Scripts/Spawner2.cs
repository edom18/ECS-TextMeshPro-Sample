using Unity.Transforms;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Rendering;
using UnityEngine;

public class Spawner2 : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private Mesh[] _meshes;
    
    private void Start()
    {
        CreateEntity();
    }

    private void CreateEntity()
    {
        World worl = World.DefaultGameObjectInjectionWorld;
        EntityManager entityManage = worl.EntityManager;
        
        RenderFilterSettings filterSetting = RenderFilterSettings.Default;
        filterSetting.ShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        filterSetting.ReceiveShadows = false;
        
        RenderMeshArray renderMeshArray = new RenderMeshArray(new[] { _material }, _meshes);
        RenderMeshDescription renderMeshDescription = new RenderMeshDescription
        {
            FilterSettings = filterSetting,
            LightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off,
        };

        // Mesh mesh = _meshes[0];
        // Vector3[] vertices = mesh.vertices;
        // for (int i = 0; i < vertices.Length; i++)
        // {
        //     vertices[i] = vertices[i] - Vector3.up * 0.1f;
        // }
        // mesh.vertices = vertices;
        // mesh.RecalculateBounds();
        
        RenderBounds renderBounds = new RenderBounds
        {
            Value = _meshes[0].bounds.ToAABB(),
        };
        
        Entity entity = entityManage.CreateEntity();
        entityManage.SetName(entity, "CubeMeshEntity");
        
        RenderMeshUtility.AddComponents(
            entity,
            entityManage,
            renderMeshDescription,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 1));
        
        Vector3 r = UnityEngine.Random.insideUnitSphere;
        entityManage.SetComponentData(entity, new LocalToWorld
        {
            Value = float4x4.TRS(new float3(0), quaternion.identity, new float3(0.1f)),
        });
        entityManage.SetComponentData(entity, renderBounds);
    }
}
