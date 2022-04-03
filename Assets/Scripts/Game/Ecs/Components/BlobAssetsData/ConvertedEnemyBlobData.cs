using Shared;
using Unity.Entities;

namespace Game.Ecs.Components.BlobAssetsData {
    public struct ConvertedEnemyBlobData {
        public Entity EnemyEntity;
        public EnemyType EnemyType;
    }
}