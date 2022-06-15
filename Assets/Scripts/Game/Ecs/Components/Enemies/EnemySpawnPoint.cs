using System;
using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    [Serializable]
    public struct EnemySpawnPoint : IComponentData {
        public float SpawnRadius;
    }
}