using System;
using Game.Ecs.Containers;
using Shared;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Monobehaviours {
    public class MonoBuildingsToEntitiesConverter : MonoBehaviour {
        public PreinstantiatePrefabData[] prefabDatas;

        private BlobAssetStore _assetStore;

        private void Start() {
            _assetStore = new BlobAssetStore();
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _assetStore);
            foreach (var prefabData in prefabDatas) {
                Entity ghostEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabData.ghost, settings);
                Entity buildingEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabData.prefab, settings);
                ConvertedEntitiesContainer.Entities.Add(prefabData.buildingType, 
                    new ConvertedEntityPrefabData{building = buildingEntity, ghost = ghostEntity, buildingType = prefabData.buildingType});
            }
        }

        private void OnDestroy() {
            _assetStore.Dispose();
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