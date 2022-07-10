using Unity.Entities;

namespace Game.Ecs.Components.Buildings {
    [GenerateAuthoringComponent]
    public struct TurretStateComponent : IComponentData {
        public TurretState PreviuosState { get; private set; }
        public TurretState CurrentState {
            get => CurrentStateBacking;
            set {
                PreviuosState = CurrentStateBacking;
                CurrentStateBacking = value;
            }
        }
        // it's public just because ECS codegen doesn't want to work with private fields
        public TurretState CurrentStateBacking { get; private set; }
    }

    public enum TurretState {
        Entry = 0, ScanningForEnemies = 1, ReadyToAttack = 2, Attacking = 3
    }
}