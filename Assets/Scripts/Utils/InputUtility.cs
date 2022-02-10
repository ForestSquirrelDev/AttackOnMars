using UnityEngine;

namespace Utils {
    public static class InputUtility {
        public static Vector3 MouseToWorld(Camera cam, LayerMask mask, bool showDebugInfo = false) {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, float.MaxValue, mask)) {
                if (showDebugInfo) Log(hitInfo);
                return hitInfo.point;
            }
            return Vector3.zero;
        }
        
        public static Vector3 MouseToWorld(Camera cam, bool showDebugInfo = false) {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, float.MaxValue)) {
                if (showDebugInfo) Log(hitInfo);
                return hitInfo.point;
            }
            return Vector3.zero;
        }
        
        private static void Log(RaycastHit hitInfo) {
            Debug.Log($"Info: {hitInfo.collider}. {hitInfo.point}. {hitInfo.transform}");
        }
    }
}