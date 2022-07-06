using Unity.Entities;
using Unity.Transforms;

namespace Game.Ecs.Components.Buildings {
    [GenerateAuthoringComponent]
    public struct CurrentTargetComponent : IComponentData {
        public Entity Entity;
        public LocalToWorld Ltw;
    }
}