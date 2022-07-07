using Unity.Entities;
using Unity.Transforms;

namespace Game.Ecs.Components.Buildings {
    [GenerateAuthoringComponent]
    public struct CurrentTurretTargetComponent : IComponentData {
        public Entity Entity;
        public LocalToWorld Ltw;
    }
}