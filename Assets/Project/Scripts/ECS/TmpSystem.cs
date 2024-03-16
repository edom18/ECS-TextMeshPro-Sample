using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct TmpSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MeshInstanceData>();
        state.RequireForUpdate<LocalToWorld>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        double time = SystemAPI.Time.ElapsedTime;

        foreach (var (meshData, localTransform)  in SystemAPI.Query<RefRW<MeshInstanceData>, RefRW<LocalToWorld>>())
        {
            quaternion rotation = math.mul(meshData.ValueRW.Rotation, quaternion.RotateY(10f * deltaTime));
            float3 position = meshData.ValueRW.Position;
            position += new float3(math.sin(time) * 0.1);
            meshData.ValueRW.Position = position;
            meshData.ValueRW.Rotation = rotation;
            localTransform.ValueRW.Value = float4x4.TRS(meshData.ValueRW.Position, rotation, meshData.ValueRW.Scale);
        }
    }
}