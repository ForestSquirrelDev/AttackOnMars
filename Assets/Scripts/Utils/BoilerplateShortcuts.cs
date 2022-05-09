using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Utils {
    public static class BoilerplateShortcuts {
        public static void SetMultipleComponentDatas(Entity entity, EntityManager manager, params IComponentData[] components) {
            foreach (IComponentData component in components) {
                manager.SetComponentData(entity, component);
            }
        }

        public static bool Approximately(this float f, float f2) {
            return Mathf.Approximately(f, f2);
        }

        public static bool Approximately(this Vector3 v, Vector3 v2) {
            return Mathf.Approximately(v.x, v2.x) && Mathf.Approximately(v.y, v2.y) && Mathf.Approximately(v.z, v2.z);
        }
    }
}