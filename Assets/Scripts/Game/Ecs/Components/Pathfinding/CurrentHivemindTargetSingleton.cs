using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Components.Pathfinding {
    public struct CurrentHivemindTargetSingleton : IComponentData {
        public float3 Value;
    }
}