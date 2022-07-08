using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;

namespace Game.Ecs.Systems.Spawners {
    public partial class SetTurretsReadyToAttackStateSystem : SystemBase {
		protected override void OnUpdate() {
            var enemyData = GetComponentDataFromEntity<Tag_Enemy>(true);
            Entities.WithAll<Tag_Turret>().ForEach((ref CurrentTurretStateComponent state, in CurrentTurretTargetComponent target) => {
                if (state.Value == TurretState.ReadyToAttack || state.Value == TurretState.Attacking) return;

                if (target.Entity != Entity.Null && enemyData.HasComponent(target.Entity)) {
                    state.Value = TurretState.ReadyToAttack;
                }
            }).WithReadOnly(enemyData).Schedule();
        }
    }
}