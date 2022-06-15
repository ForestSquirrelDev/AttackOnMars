using Shared;
using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct BuildingGhostComponent : IComponentData {
        // ReSharper disable once UnassignedField.Global
        public BuildingType BuildingType;
    }
}