using System;
using Game.Ecs.Containers;
using Game.Ecs.Hybrid.Conversion;
using Shared;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Ecs.Monobehaviours {
    public class MonoBuildingsToEntitiesConverter : GameObjectsConverterBase {
        [FormerlySerializedAs("prefabDatas"), SerializeField] private PreinstantiatePrefabData[] _prefabDatas;

        private World _world;
        private BlobAssetStore _blobAssetStore;

        public override void Convert() {
            _world = World.DefaultGameObjectInjectionWorld;
            _blobAssetStore = new BlobAssetStore();
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
            foreach (var prefabData in _prefabDatas) {
                Entity ghostEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabData.Ghost, settings);
                Entity buildingEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabData.Prefab, settings);
                _world.EntityManager.SetName(ghostEntity, $"Converted <{prefabData.BuildingType}> ghost");
                _world.EntityManager.SetName(buildingEntity, $"Converted <{prefabData.BuildingType}> building");
                ConvertedEntitiesContainer.s_Entities.Add(prefabData.BuildingType, 
                    new ConvertedEntityPrefabData{Building = buildingEntity, Ghost = ghostEntity, BuildingType = prefabData.BuildingType});
            }
        }

        private void OnDestroy() {
            _blobAssetStore.Dispose();
        }

        [Serializable]
        public struct PreinstantiatePrefabData {
            [FormerlySerializedAs("prefab")] public GameObject Prefab;
            [FormerlySerializedAs("ghost")] public GameObject Ghost;
            [FormerlySerializedAs("buildingType")] public BuildingType BuildingType;
        }

        public struct ConvertedEntityPrefabData {
            public Entity Building;
            public Entity Ghost;
            public BuildingType BuildingType;
        }
    }
}