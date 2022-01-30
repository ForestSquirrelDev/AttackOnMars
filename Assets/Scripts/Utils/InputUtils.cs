using UnityEngine;

namespace Utils {
    public static class InputUtils {
        public static Vector3 MouseToWorld(Camera cam, LayerMask mask, bool showDebugInfo = true) {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, float.MaxValue, mask)) {
                if (showDebugInfo) Debug.Log($"Info: {hitInfo.collider}. {hitInfo.point}. {hitInfo.transform}");
                return hitInfo.point;
            }
            return Vector3.zero;
        }
    }
}