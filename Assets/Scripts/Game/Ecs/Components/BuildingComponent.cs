using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct BuildingComponent : IComponentData {
        public bool inited;
        public PositioningQuadComponent positioningQuad;
    }
}