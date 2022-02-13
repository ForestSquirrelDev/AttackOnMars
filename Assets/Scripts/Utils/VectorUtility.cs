using Unity.Mathematics;
using UnityEngine;

namespace Utils {
    public static class VectorUtility {
        public static Vector3 FloorToInt(this Vector3 v) {
            return new Vector3(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
        }

        public static Vector3 CenterXZ(this Vector3 v, bool floorToInt = true) {
            if (floorToInt) v = v.FloorToInt();
            return new Vector3(v.x + 0.5f, v.y, v.z + 0.5f);
        }

        public static Vector2 ToVector2XZ(this Vector3 v) {
            return new Vector2(v.x, v.z);
        }

        public static Vector3 ToVector3XZ(this Vector2Int v) {
            return new Vector3(v.x, 0f, v.y);
        }
        
        public static Vector3 ToVector3XZ(this Vector3 v) {
            return new Vector3(v.x, 0f, v.y);
        }

        public static Vector2Int RoundToVector2IntXZ(this Vector3 v) {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.z));
        }

        public static Vector2Int FloorToVector2IntXZ(this Vector3 v) {
            return new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.z));
        }

        public static Vector2Int CeilToVector2IntXZ(this Vector3 v) {
            return new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.z));
        }

        public static Vector4 ToVector4(this float3 f3, float w = 0) {
            return new Vector4(f3.x, f3.y, f3.z, w);
        }

        public static Vector4 ToVector4(this float4 f4) {
            return new Vector4(f4.x, f4.y, f4.z, f4.w);
        }

        public static float4 Normalized(this float4 f4) {
            return f4.ToVector4().normalized;
        }

        public static int2 ToInt2(this Vector2Int v) {
            return new int2(v.x, v.y);
        }

        public static float4 ToFloat4(this Vector4 v) {
            return new float4(v.x, v.y, v.z, v.w);
        }

        public static Vector2Int ToVector2Int(this int2 value) {
            return new Vector2Int(value.x, value.y);
        }
    }
}