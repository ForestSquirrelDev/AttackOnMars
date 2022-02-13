using System;
using Game.Ecs.Containers;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Monobehaviours {
    public class MonoBuildingsToEntitiesConverter : MonoBehaviour {
        public PreinstantiatePrefabData[] prefabDatas;
        private EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        
        private void Start() {
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            foreach (var prefabData in prefabDatas) {
                Entity ghostEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabData.ghost, settings);
                Entity buildingEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabData.prefab, settings);
                ConvertedEntitiesContainer.entities.Add(prefabData.buildingType, 
                    new ConvertedEntityPrefabData{building = buildingEntity, ghost = ghostEntity, buildingType = prefabData.buildingType});
            }
        }

        [Serializable]
        public struct PreinstantiatePrefabData {
            public GameObject prefab;
            public GameObject ghost;
            public BuildingType buildingType;
        }

        public struct ConvertedEntityPrefabData {
            public Entity building;
            public Entity ghost;
            public BuildingType buildingType;
        }
    }
}