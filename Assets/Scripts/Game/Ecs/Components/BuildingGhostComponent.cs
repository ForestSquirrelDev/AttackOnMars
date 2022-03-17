using Shared;
using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct BuildingGhostComponent : IComponentData {
        public BuildingType BuildingType;
    }
}