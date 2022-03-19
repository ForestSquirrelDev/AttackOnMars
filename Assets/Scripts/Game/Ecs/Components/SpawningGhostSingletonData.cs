using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct SpawningGhostSingletonData : IComponentData {
        public bool CanSpawn;
    }
}