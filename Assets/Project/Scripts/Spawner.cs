using Unity.Entities;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Entities.Graphics;
using Unity.Rendering;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int SpawnNum = 100;
    public float SpawnRange = 5f;
    public Material Material;
    public Mesh[] Meshes;

    public void Start()
    {
        EntityCreate();
    }

    private void EntityCreate()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;

        int digitNum = (int)Mathf.Log10(SpawnNum) + 1;
        string digitFormat = $"D{digitNum}";

        var filterSettings = RenderFilterSettings.Default;
        filterSettings.ShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        filterSettings.ReceiveShadows = false;

        var renderMeshArray = new RenderMeshArray(new[] { Material }, Meshes);
        var renderMeshDescription = new RenderMeshDescription
        {
            FilterSettings = filterSettings,
            LightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off,
        };

        var renderBounds = new RenderBounds[Meshes.Length];
        for (int i = 0; i < Meshes.Length; i++)
        {
            renderBounds[i] = new RenderBounds { Value = Meshes[i].bounds.ToAABB() };
        }

        for (int i = 0; i < SpawnNum; i++)
        {
            var entity = entityManager.CreateEntity();
            entityManager.SetName(entity, string.Format("MeshEntity{0}", i.ToString(digitFormat)));

            var meshIndex = i % Meshes.Length;

            RenderMeshUtility.AddComponents(
                entity,
                entityManager,
                renderMeshDescription,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, meshIndex));

            var randPos = UnityEngine.Random.insideUnitCircle * SpawnRange;
            entityManager.SetComponentData(entity, new LocalToWorld
                {
                    Value = float4x4.TRS(new float3(randPos.x, 0f, randPos.y), quaternion.identity, new float3(0.1f))
                });
            entityManager.SetComponentData(entity, renderBounds[meshIndex]);
        }
    }
}