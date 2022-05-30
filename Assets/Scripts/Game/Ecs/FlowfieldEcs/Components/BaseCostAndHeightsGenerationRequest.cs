using Unity.Entities;

namespace Game.Ecs.Flowfield.Components {
    public struct BaseCostAndHeightsGenerationRequest : IComponentData {
        public Entity Entity;
        public bool IsProcessing;
    }
}