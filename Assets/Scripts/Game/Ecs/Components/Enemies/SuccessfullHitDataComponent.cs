using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct SuccessfullHitDataComponent : IComponentData {
        public bool HasHit;
        public Entity Entity;
        public float3 ContactPoint;
        public float Damage;
    }
}