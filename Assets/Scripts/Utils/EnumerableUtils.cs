using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
            Debug.LogWarning("Couldn't find entity with component of type " + nameof(T));
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
            Debug.LogWarning("Couldn't find entity with component of type " + nameof(T));
            componentData = null;
            return Entity.Null;
        }

        public static List<Entity> FindAllEntitiesWithComponent<T>(this Entity entity, EntityManager manager) where T : struct, IComponentData {
            var entityGroups = manager.GetBuffer<LinkedEntityGroup>(entity);
            var entitiesWithComponent = new List<Entity>();
            for (int i = 0; i < entityGroups.Length; i++) {
                if (manager.HasComponent<T>(entityGroups[i].Value)) {
                    entitiesWithComponent.Add(entityGroups[i].Value);
                }
            }
            return entitiesWithComponent;
        }

        [return: ReadOnly]
        public static NativeList<T> Reverse<T>(this NativeList<T> oldList, Allocator allocator) where T : unmanaged {
            var newList = new NativeList<T>(oldList.Length, allocator);
            var crapCodeDefense = oldList.Length;
            
            while (oldList.Length > 0) {
                var item = oldList[oldList.Length - 1];
                oldList.RemoveAt(oldList.Length - 1);
                newList.Add(item);
                crapCodeDefense--;
                if (crapCodeDefense < 0) {
                    Debug.LogError("Infinite loop");
                    break;
                }
            }

            oldList.Dispose();
            return newList;
        }

        public static T Pop<T>(this NativeList<T> list) where T : unmanaged {
            if (list.IsEmpty) {
                Debug.LogError("List is empty");
                return default;
            }
            int index = list.Length - 1;
            var item = list[index];
            list.RemoveAt(index);
            return item;
        }
    }
}