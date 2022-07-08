using Unity.Entities;

namespace Game.Ecs.Components.Buildings {
    [GenerateAuthoringComponent]
    public struct RadarTickCounterComponent : IComponentData {
        public float Value;
    }
}