using Game.Ecs.Components.BlobAssetsData;
using Unity.Entities;

public struct ConvertedEnemiesBlobAssetReference : IComponentData {
    public BlobAssetReference<ConvertedEnemiesBlobAsset> EnemiesReference;
}