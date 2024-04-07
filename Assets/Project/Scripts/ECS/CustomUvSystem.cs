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

        foreach (var customUvData in SystemAPI.Query<RefRW<CustomUvData>>())
        {
            double t = math.abs(math.sin(time));
            // customUvData.ValueRW.Value = new float4(0, 0, 0.1f * (float)t, 0.1f);
        }
    }
}

