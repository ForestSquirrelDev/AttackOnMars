using Unity.Entities;

namespace Game.Ecs.Components.Enemies {
    [GenerateAuthoringComponent]
    public struct EnemyStateComponent : IComponentData {
        public EnemyState Value;
    }

    public enum EnemyState {
        // the entity's just spawned. in the next update it will be set to moving
        Entry = 0,
        // entity is moving towards base
        Moving = 1, 
        // entity is near the base but not near enough to attack it. still moving
        ReadyToAttack = 2, 
        // not moving. just attacking
        Attacking = 3,
        // the base is destroyed. do nothing
        SucceededToWin = 4
    }
}