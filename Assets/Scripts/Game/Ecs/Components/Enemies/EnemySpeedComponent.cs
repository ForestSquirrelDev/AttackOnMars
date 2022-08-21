using Unity.Entities;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct EnemySpeedComponent : IComponentData {
        public float Value;
    }
}