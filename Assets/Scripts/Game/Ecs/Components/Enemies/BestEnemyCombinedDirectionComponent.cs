using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct BestEnemyCombinedDirectionComponent : IComponentData {
        public float3 Value;
    }
}