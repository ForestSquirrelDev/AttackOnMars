using Unity.Entities;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct GridDirectionUpdateSkipCounterComponent : IComponentData {
        public int Value;
    }
}