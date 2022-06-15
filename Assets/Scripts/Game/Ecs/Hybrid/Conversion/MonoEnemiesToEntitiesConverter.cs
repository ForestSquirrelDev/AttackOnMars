using System;
using Game.Ecs.Components.BlobAssetsData;
using Shared;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Hybrid.Conversion {
    public class MonoEnemiesToEntitiesConverter : GameObjectsConverterBase {
        [SerializeField] private PreinstantiatePrefabData[] _prefabDatas;
    
        private World _world => World.DefaultGameObjectInjectionWorld;
        private BlobAssetStore _blobAssetStore;

        public override void Convert() {
            _blobAssetStore = new BlobAssetStore();
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _blobAssetStore);
            var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var convertedEnemiesBlobAsset = ref blobBuilder.ConstructRoot<ConvertedEnemiesBlobAsset>();
            var convertedEnemiesArray = blobBuilder.Allocate(ref convertedEnemiesBlobAsset.EnemiesArray, _prefabDatas.Length);

            for (int i = 0; i < _prefabDatas.Length; i++) {
                var prefabData = _prefabDatas[i];
                var enemyEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabData.Prefab, settings);
                _world.EntityManager.SetName(enemyEntity, $"Converted <{prefabData.EnemyType}> enemy");
                convertedEnemiesArray[i] = new ConvertedEnemyBlobData {
                    EnemyEntity = enemyEntity, 
                    EnemyType = prefabData.EnemyType
                };
            }

            var blobAssetReferenceComponent = new ConvertedEnemiesBlobAssetReference {
                EnemiesReference = blobBuilder.CreateBlobAssetReference<ConvertedEnemiesBlobAsset>(Allocator.Persistent)
            };
            var referenceSingleton = _world.EntityManager.CreateEntity(ComponentType.ReadWrite<ConvertedEnemiesBlobAssetReference>());
            _world.EntityManager.SetComponentData(referenceSingleton, blobAssetReferenceComponent);
        
            blobBuilder.Dispose();
        }

        private void OnDestroy() {
            _blobAssetStore.Dispose();
        }

        [Serializable]
        private struct PreinstantiatePrefabData {
            public GameObject Prefab;
            public EnemyType EnemyType;
        }
    }
}