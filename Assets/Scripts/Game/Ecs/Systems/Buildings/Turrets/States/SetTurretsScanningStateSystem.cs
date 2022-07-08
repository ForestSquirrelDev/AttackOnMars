using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;

namespace Game.Ecs.Systems.Spawners {
    public partial class SetTurretsScanningStateSystem : SystemBase {
		protected override void OnUpdate() {
            Entities.WithAll<Tag_Turret>().ForEach((ref CurrentTurretStateComponent state, in CurrentTurretTargetComponent target) => {
                if (state.Value == TurretState.ScanningForEnemies) return;

                if (target.Entity == Entity.Null) {
                    state.Value = TurretState.ScanningForEnemies;
                }
            }).Schedule();
        }
    }
}