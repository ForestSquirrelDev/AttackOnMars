using Unity.Entities;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct EnemyStateComponent : IComponentData {
        public EnemyState State;
        public float ArrivedRaycastCheckCounter;
    }

    public enum EnemyState {
        Entry = 0, Moving = 1, ReadyToAttack = 2, Attacking = 3, Dead = 4
    }
}