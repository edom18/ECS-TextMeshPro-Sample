using Unity.Entities;
using Unity.Mathematics;

public partial struct CustomUvSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CustomUvData>();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        double time = SystemAPI.Time.ElapsedTime;

        float u = 0f;
        foreach (var customUvData in SystemAPI.Query<RefRW<CustomUvData>>())
        {
            customUvData.ValueRW.Value = new float2(math.clamp(u + (float)time, 0f, 1f), 0.5f);
            u += 0.001f;
        }
    }
}

