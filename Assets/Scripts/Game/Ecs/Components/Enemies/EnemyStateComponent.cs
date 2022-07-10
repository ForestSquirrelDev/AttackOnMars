using Unity.Entities;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct EnemyStateComponent : IComponentData {
        public EnemyState Value;
    }

    public enum EnemyState {
        Entry = 0, Moving = 1, ReadyToAttack = 2, Attacking = 3, SucceededToWin = 4
    }
}