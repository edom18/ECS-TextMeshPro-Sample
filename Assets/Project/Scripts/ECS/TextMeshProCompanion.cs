using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TextMeshProCompanion : MonoBehaviour
{
    private class Baker : Baker<TextMeshProCompanion>
    {
        public override void Bake(TextMeshProCompanion companion)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}