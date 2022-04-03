using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct EnemiesSingletonData : IComponentData {
        public int MaxPathSize;
        public int MaxEntitiesRoutedPerFrame;
        public int MaxPathNodePoolSize;
        public int MaxIterations;
        public int SpawnFrequency;
        public bool UseCache;
    }
}
