using Unity.Entities;

namespace Game.Ecs.Components.Buildings {
    [GenerateAuthoringComponent]
    public struct RotatableTurretPartReferenceComponent : IComponentData {
        public Entity Value;
    }
}