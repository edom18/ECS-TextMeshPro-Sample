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
            Time = SystemAPI.Time.ElapsedTime,
        };
        job.ScheduleParallel();
    }
}

[BurstCompile]
partial struct TmpUpdateJob : IJobEntity
{
    public double Time;

    private void Execute([EntityIndexInQuery] int index, ref MeshInstanceData meshData, ref LocalToWorld localTransform)
    {
        double move = math.sin((Time * meshData.TimeSpeed + index) * math.PI) * meshData.MoveSpan; // index is just offset for the time.
        float3 position = meshData.Position + new float3(move);

        float angleSpeed = 0.005f;
        float angle = (float)math.sin(Time * meshData.TimeSpeed * angleSpeed * math.PI) * 360f;
        quaternion rotation = math.mul(meshData.Rotation, quaternion.RotateY(angle));
        
        localTransform.Value = float4x4.TRS(position, rotation, meshData.Scale);
    }
}