using Unity.Entities;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct HumanBaseDetectionTickCounterComponent : IComponentData {
        public float Value;
    }
}