using Unity.Entities;
using UnityEngine;

namespace Utils {
    public static class EnumerableUtils {
        public static Entity ReverseFindEntityWithComponent<T>(this Entity entity, EntityManager manager) where T : IComponentData {
            var entityGroups = manager.GetBuffer<LinkedEntityGroup>(entity);
            for (int i = entityGroups.Length - 1; i >= 0; i--) {
                if (manager.HasComponent<T>(entityGroups[i].Value)) {
                    return entityGroups[i].Value;
                }
            }
            Debug.LogWarning("Couldn't find entity with component of type" + nameof(T));
            return Entity.Null;
        }
        
        public static Entity ReverseFindEntityWithComponent<T>(this Entity entity, EntityManager manager, out IComponentData componentData) where T : struct, IComponentData {
            var entityGroups = manager.GetBuffer<LinkedEntityGroup>(entity);
            for (int i = entityGroups.Length - 1; i >= 0; i--) {
                if (manager.HasComponent<T>(entityGroups[i].Value)) {
                    componentData = manager.GetComponentData<T>(entityGroups[i].Value);
                    return entityGroups[i].Value;
                }
            }
            Debug.LogWarning("Couldn't find entity with component of type" + nameof(T));
            componentData = null;
            return Entity.Null;
        }
    }
}