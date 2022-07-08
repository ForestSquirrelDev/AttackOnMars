using Unity.Entities;

namespace Game.Ecs.Components.Buildings {
    [GenerateAuthoringComponent]
    public struct TurretMuzzelFlashAnchorComponent : IComponentData {
        public Entity Value;
    }
}