using Unity.Entities;

namespace Game.Ecs.Components.BlobAssetsData {
    public struct ConvertedEnemiesBlobAsset {
        public BlobArray<ConvertedEnemyBlobData> EnemiesArray;
    }
}