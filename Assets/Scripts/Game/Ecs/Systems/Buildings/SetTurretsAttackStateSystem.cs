using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Ecs.Systems.Spawners {
    public partial class SetTurretsAttackStateSystem : SystemBase {
        private TurretsConfig _turretsConfig;

        protected override void OnCreate() {
            _turretsConfig = ConfigsLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
        }

        protected override void OnUpdate() {
            var lookAtEnemyError = _turretsConfig.LookAtEnemyError;
            var localToWorldData = GetComponentDataFromEntity<LocalToWorld>(true);
            Entities.WithAll<Tag_Turret>().ForEach((ref CurrentTurretStateComponent state, in CurrentTurretTargetComponent target, in RotatableTurretPartsReferenceComponent rotatable) => {
                if (state.Value != TurretState.ReadyToAttack) return;
                
                var rotatableLtw = localToWorldData[rotatable.BaseRotation];
                var directionTowardsEnemy = target.Ltw.Position - rotatableLtw.Position;
                var dot = math.dot(rotatableLtw.Forward, math.normalizesafe(directionTowardsEnemy));
                if (dot >= lookAtEnemyError) {
                    state.Value = TurretState.Attacking;
                }
            }).WithReadOnly(localToWorldData).Schedule();
        }
    }
}