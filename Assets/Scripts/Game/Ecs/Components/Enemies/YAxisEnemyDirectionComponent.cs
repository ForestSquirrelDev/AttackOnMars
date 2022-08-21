using Unity.Entities;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct YAxisEnemyDirectionComponent : IComponentData {
        public float Value;
    }
}