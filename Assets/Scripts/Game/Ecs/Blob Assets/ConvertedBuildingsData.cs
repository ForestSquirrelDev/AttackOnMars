using Unity.Entities;

namespace Game.Ecs.BlobAssets {
    public struct ConvertedBuildingsData : IComponentData {
        public BlobArray<ConvertedBuildingData> buildingsArray;
    }
}