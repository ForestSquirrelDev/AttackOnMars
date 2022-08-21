using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Components.Pathfinding {
    [GenerateAuthoringComponent]
    public struct BestEnemyGridDirectionComponent : IComponentData {
        public float2 Value;
    }
}