using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
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
        var job = new TmpUpdateJob()
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            Time = SystemAPI.Time.ElapsedTime
        };
        job.ScheduleParallel();
    }
}

[BurstCompile]
partial struct TmpUpdateJob : IJobEntity
{
    public float DeltaTime;
    public double Time;

    void Execute(ref MeshInstanceData meshData, ref LocalToWorld localTransform)
    {
        quaternion rotation = math.mul(meshData.Rotation, quaternion.RotateY(10f * DeltaTime));
        float3 position = meshData.Position;
        position += new float3(math.sin(Time) * 0.1);
        meshData.Position = position;
        meshData.Rotation = rotation;
        localTransform.Value = float4x4.TRS(meshData.Position, meshData.Rotation, meshData.Scale);
    }
}