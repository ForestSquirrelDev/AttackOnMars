using Unity.Entities;

namespace Game.Ecs.BlobAssets {
    public struct ConvertedBuildingData {
        public Entity building;
        public Entity ghost;
        public BuildingType buildingType;
    }
}