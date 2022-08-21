using Unity.Entities;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct LocalAvoidanceTickCounterComponent : IComponentData {
        public int Value;
    }
}