using Unity.Entities;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct AttackCounterComponent : IComponentData {
        public float Value;
    }
}