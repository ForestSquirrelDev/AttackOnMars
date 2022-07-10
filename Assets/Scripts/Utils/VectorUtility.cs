using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Utils {
    public static class VectorUtility {
        [return: ReadOnly]
        public static Vector3 FloorToInt(this Vector3 v) {
            return new Vector3(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
        }

        [return: ReadOnly]
        public static Vector3 CenterXZ(this Vector3 v, bool floorToInt = true) {
            if (floorToInt) v = v.FloorToInt();
            return new Vector3(v.x + 0.5f, v.y, v.z + 0.5f);
        }

        [return: ReadOnly]
        public static Vector2 ToVector2XZ(this Vector3 v) {
            return new Vector2(v.x, v.z);
        }

        [return: ReadOnly]
        public static Vector3 ToVector3XZ(this Vector2Int v) {
            return new Vector3(v.x, 0f, v.y);
        }
        
        [return: ReadOnly]
        public static Vector3 ToVector3XZ(this Vector3 v) {
            return new Vector3(v.x, 0f, v.y);
        }

        [return: ReadOnly]
        public static Vector2Int RoundToVector2IntXZ(this Vector3 v) {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.z));
        }

        [return: ReadOnly]
        public static Vector2Int FloorToVector2IntXZ(this Vector3 v) {
            return new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.z));
        }
        
        [return: ReadOnly]
        public static Vector2Int FloorToVector2IntXZ(this float4 f) {
            return new Vector2Int(Mathf.FloorToInt(f.x), Mathf.FloorToInt(f.z));
        }
        
        [return: ReadOnly]
        public static Vector2Int FloorToVector2IntXZ(this float3 f) {
            return new Vector2Int(Mathf.FloorToInt(f.x), Mathf.FloorToInt(f.z));
        }

        [return: ReadOnly]
        public static Vector2Int CeilToVector2IntXZ(this Vector3 v) {
            return new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.z));
        }

        [return: ReadOnly]
        public static Vector2Int CeilToVector2IntXZ(this float4 f) {
            return new Vector2Int(Mathf.CeilToInt(f.x), Mathf.CeilToInt(f.z));
        }
        
        [return: ReadOnly]
        public static Vector2Int CeilToVector2IntXZ(this float3 f) {
            return new Vector2Int(Mathf.CeilToInt(f.x), Mathf.CeilToInt(f.z));
        }

        [return: ReadOnly]
        public static Vector4 ToVector4(this float3 f3, float w = 0) {
            return new Vector4(f3.x, f3.y, f3.z, w);
        }

        [return: ReadOnly]
        public static Vector4 ToVector4(this float4 f4) {
            return new Vector4(f4.x, f4.y, f4.z, f4.w);
        }

        [return: ReadOnly]
        public static float4 Normalized(this float4 f4) {
            return f4.ToVector4().normalized;
        }
        
        [return: ReadOnly]
        public static int2 ToInt2(this Vector2Int v) {
            return new int2(v.x, v.y);
        }

        [return: ReadOnly]
        public static float4 ToFloat4(this Vector4 v) {
            return new float4(v.x, v.y, v.z, v.w);
        }

        [return: ReadOnly]
        public static float3 ToFloat3(this Vector4 v) {
            return new float3(v.x, v.y, v.z);
        }
        
        [return: ReadOnly]
        public static float3 ToFloat3(this float4 f) {
            return new float3(f.x, f.y, f.z);
        }

        [return: ReadOnly]
        public static Vector2Int ToVector2Int(this int2 value) {
            return new Vector2Int(value.x, value.y);
        }

        [return: ReadOnly]
        public static float4 ToFloat4(this Vector3 v, float w = 0) {
            return new float4(v.x, v.y, v.z, w);
        }

        [return: ReadOnly]
        public static Vector3 MaxValue() {
            return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        }
        
        public static float3 Invalid() {
            return new float3(float.MinValue, float.MinValue, float.MinValue);
        }

        public static float3 Up(this float3 f) {
            return new float3(0, 1, 0);
        }
        
        public static bool Approximately(this Vector3 v, Vector3 v2) {
            return Mathf.Approximately(v.x, v2.x) && Mathf.Approximately(v.y, v2.y) && Mathf.Approximately(v.z, v2.z);
        }
    }
}