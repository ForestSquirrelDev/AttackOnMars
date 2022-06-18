using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

namespace Game.Ecs.Hybrid.Conversion {
    public class ConvertElonBaseToEntity : GameObjectsConverterBase {
        [SerializeField] private GameObject _base;

        private BlobAssetStore _assetStore;
        
        public override void Convert() {
            _assetStore = new BlobAssetStore();
            var conversionSettings = GameObjectConversionSettings.FromWorld(World, _assetStore);
            var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(_base, conversionSettings);
            // physics body is added here because conversion for some reason doesn't want to grab it from the gameobject authoring component
            EntityManager.AddComponent<PhysicsBodyAuthoring>(entity);
            EntityManager.SetName(entity, "ElonMuskBase");
            Destroy(_base);
        }

        private void OnDestroy() {
            _assetStore.Dispose();
        }
    }
}