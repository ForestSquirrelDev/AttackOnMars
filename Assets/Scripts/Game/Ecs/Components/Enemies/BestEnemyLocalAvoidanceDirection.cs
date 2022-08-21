using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct BestEnemyLocalAvoidanceDirection : IComponentData {
        public float2 Value;
    }
}