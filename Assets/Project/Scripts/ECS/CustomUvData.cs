using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[MaterialProperty("_CustomUv")]
public struct CustomUvData : IComponentData
{
    public float4 Value;
}
