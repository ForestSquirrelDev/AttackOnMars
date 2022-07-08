using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct BuildingGhostPositioningQuadComponent : IComponentData {
        public bool AvailableForPlacement;
    }
}