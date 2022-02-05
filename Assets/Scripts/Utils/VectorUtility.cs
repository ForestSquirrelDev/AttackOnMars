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
    }
}