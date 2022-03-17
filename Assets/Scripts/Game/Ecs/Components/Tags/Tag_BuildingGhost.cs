using Shared;
using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct Tag_BuildingGhost : IComponentData {
        public BuildingType BuildingType;
    }
}