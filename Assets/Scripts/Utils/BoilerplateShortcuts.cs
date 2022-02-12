using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utils;

namespace Game.Ecs.Utils {
    public static class BoilerplateShortcuts {
        public static void SetMultipleComponentDatas(Entity entity, EntityManager manager, params IComponentData[] components) {
            foreach (IComponentData component in components) {
                manager.SetComponentData(entity, component);
            }
        }
    }
}