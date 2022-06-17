using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct HealthComponent : IComponentData {
        public int CurrentHealth;
    }
}