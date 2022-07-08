using Unity.Entities;

namespace Game.Ecs.Components.Buildings {
    [GenerateAuthoringComponent]
    public struct TurretBarrelCurrentRotationComponent : IComponentData {
        public float CurrentSpeed;
        public float Angle;
    }
}