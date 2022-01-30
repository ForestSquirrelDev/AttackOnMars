using UnityEngine;

namespace Utils {
    public static class VectorUtility {
        public static Vector3 FloorToInt(this Vector3 v) {
            return new Vector3(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
        }

        public static Vector3 CenterXZ(this Vector3 v, bool floorToInt = true) {
            if (floorToInt) v.FloorToInt();
            return new Vector3(v.x + 0.5f, v.y, v.z + 0.5f);
        }

        public static Vector2 ToXZ(this Vector3 v) {
            return new Vector2(v.x, v.z);
        }

        public static Vector3Int RoundToVector3Int(this Vector3 v) {
            return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }
    }
}