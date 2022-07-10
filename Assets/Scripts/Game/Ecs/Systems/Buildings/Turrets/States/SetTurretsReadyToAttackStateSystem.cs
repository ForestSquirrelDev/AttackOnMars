using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;

namespace Game.Ecs.Systems.Spawners {
    public partial class SetTurretsReadyToAttackStateSystem : SystemBase {
		protected override void OnUpdate() {
            var enemyData = GetComponentDataFromEntity<Tag_Enemy>(true);
            Entities.WithAll<Tag_Turret>().ForEach((ref TurretStateComponent state, in CurrentTurretTargetComponent target) => {
                if (state.CurrentState == TurretState.ReadyToAttack || state.CurrentState == TurretState.Attacking) return;

                if (target.Entity != Entity.Null && enemyData.HasComponent(target.Entity)) {
                    state.CurrentState = TurretState.ReadyToAttack;
                }
            }).WithReadOnly(enemyData).Schedule();
        }
    }
}