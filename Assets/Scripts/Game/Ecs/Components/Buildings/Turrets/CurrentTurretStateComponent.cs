using Unity.Entities;

namespace Game.Ecs.Components.Buildings {
    [GenerateAuthoringComponent]
    public struct CurrentTurretStateComponent : IComponentData {
        public TurretState Value;
    }

    public enum TurretState {
        Entry = 0, ScanningForEnemies = 1, ReadyToAttack = 2, Attacking = 3
    }
}