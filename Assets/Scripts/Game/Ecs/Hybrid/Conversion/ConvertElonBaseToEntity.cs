using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Hybrid.Conversion {
    public class ConvertElonBaseToEntity : GameObjectsConverterBase {
        [SerializeField] private GameObject _base;

        private BlobAssetStore _assetStore;
        
        public override void Convert() {
            _assetStore = new BlobAssetStore();
            var conversionSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _assetStore);
            var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(_base, conversionSettings);
            World.DefaultGameObjectInjectionWorld.EntityManager.SetName(entity, "ElonMuskBase");
            Destroy(_base);
        }

        private void OnDestroy() {
            _assetStore.Dispose();
        }
    }
}