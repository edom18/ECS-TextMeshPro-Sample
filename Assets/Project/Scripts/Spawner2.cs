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
        
        RenderBounds renderBounds = new RenderBounds
        {
            Value = _meshes[0].bounds.ToAABB(),
        };
        
        Entity entity = entityManage.CreateEntity();
        entityManage.SetName(entity, "MeshEntity");
        
        RenderMeshUtility.AddComponents(
            entity,
            entityManage,
            renderMeshDescription,
            renderMeshArray,
            MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        
        Vector3 r = UnityEngine.Random.insideUnitSphere;
        entityManage.SetComponentData(entity, new LocalToWorld
        {
            Value = float4x4.TRS(new float3(r.x, r.y, r.z), quaternion.identity, new float3(0.1f)),
        });
        entityManage.SetComponentData(entity, renderBounds);
    }
}
