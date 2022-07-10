using System;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils.HelperAttributes;

namespace Utils {
    public static class HybridEcsExtensions {
        [OffMainThreadUsage(OffMainThreadUsage.Disallowed)]
        public static void SetAsChildOfEntityWithOffset(this Transform t, LocalToWorld parentEntityLocalToWorld, float3 offset) {
            var position = Matrix4x4Extensions.LocalOffsetToWorldPoint(parentEntityLocalToWorld.Value, offset);
            var rotation = parentEntityLocalToWorld.Value.GetRotation();
            var scale = parentEntityLocalToWorld.Value.GetScale();
            
            t.position = position.xyz;
            t.rotation = rotation;
            t.localScale = scale;
        }
    }
}