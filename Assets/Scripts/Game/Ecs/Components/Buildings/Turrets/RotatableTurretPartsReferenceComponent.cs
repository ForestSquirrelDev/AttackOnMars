using Unity.Entities;

namespace Game.Ecs.Components.Buildings {
    [GenerateAuthoringComponent]
    public struct RotatableTurretPartsReferenceComponent : IComponentData {
        public Entity BaseRotation;
        public Entity Barrel;
    }
}