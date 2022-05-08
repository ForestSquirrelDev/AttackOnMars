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
    }
}