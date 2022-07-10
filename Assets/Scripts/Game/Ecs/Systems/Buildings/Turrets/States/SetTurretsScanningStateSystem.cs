using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;

namespace Game.Ecs.Systems.Spawners {
    public partial class SetTurretsScanningStateSystem : SystemBase {
		protected override void OnUpdate() {
            Entities.WithAll<Tag_Turret>().ForEach((ref TurretStateComponent state, in CurrentTurretTargetComponent target) => {
                if (state.CurrentState == TurretState.ScanningForEnemies) return;

                if (target.Entity == Entity.Null) {
                    state.CurrentState = TurretState.ScanningForEnemies;
                }
            }).Schedule();
        }
    }
}