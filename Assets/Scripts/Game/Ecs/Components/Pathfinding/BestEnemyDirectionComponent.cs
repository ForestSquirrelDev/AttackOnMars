using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Components.Pathfinding {
    [GenerateAuthoringComponent]
    public struct BestEnemyDirectionComponent : IComponentData {
        public float3 Value;
    }
}