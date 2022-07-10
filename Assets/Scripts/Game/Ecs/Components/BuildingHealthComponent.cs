using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct BuildingHealthComponent : IComponentData {
        public int Value;
    }
}