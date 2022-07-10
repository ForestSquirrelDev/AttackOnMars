using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems.Spawners {
    public partial class SetTurretsAttackingStateSystem : SystemBase {
        private TurretsConfig _turretsConfig;

        protected override void OnCreate() {
            _turretsConfig = AddressablesLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
        }

        protected override void OnUpdate() {
            var lookAtEnemyError = _turretsConfig.LookAtEnemyError;
            var localToWorldData = GetComponentDataFromEntity<LocalToWorld>(true);
            Entities.WithAll<Tag_Turret>().ForEach((ref TurretStateComponent state, in CurrentTurretTargetComponent target, in RotatableTurretPartsReferenceComponent rotatable) => {
                if (state.CurrentState != TurretState.ReadyToAttack) return;
                
                var rotatableLtw = localToWorldData[rotatable.BaseRotation];
                var directionTowardsEnemy = target.Ltw.Position - rotatableLtw.Position;
                var dot = math.dot(math.normalizesafe(rotatableLtw.Forward), math.normalizesafe(directionTowardsEnemy));
                
                if (dot >= lookAtEnemyError) {
                    state.CurrentState = TurretState.Attacking;
                }
            }).WithReadOnly(localToWorldData).Schedule();
        }
    }
}