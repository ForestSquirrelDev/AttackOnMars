using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems.Spawners {
    public partial class TurretsAttackSystem : SystemBase {
        private TurretsConfig _turretsConfig;

        protected override void OnCreate() {
            _turretsConfig = ConfigsLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
        }

        protected override void OnUpdate() {
            var localToWorldData = GetComponentDataFromEntity<LocalToWorld>(true);
            var enemyHealthData = GetComponentDataFromEntity<EnemyHealthComponent>(false);
            var rotationError = _turretsConfig.AttackRotationError;
            var damage = _turretsConfig.Damage;
            var attacksPerUpdate = _turretsConfig.AttacksPerUpdate;
                
            Entities.WithAll<Tag_Turret>().ForEach((in RotatableTurretPartReferenceComponent rotatable, in CurrentTargetComponent currentEnemyTarget) => {
                var rotatableLtw = localToWorldData[rotatable.Value];
                var directionTowardsEnemy = currentEnemyTarget.Ltw.Position - rotatableLtw.Position;
                var dot = math.dot(rotatableLtw.Forward, math.normalizesafe(directionTowardsEnemy));
                if (dot < rotationError) return;

                var enemyHealth = enemyHealthData[currentEnemyTarget.Entity];
                for (int i = 0; i < attacksPerUpdate; i++) {
                    enemyHealth.Value -= damage;
                }
                enemyHealthData[currentEnemyTarget.Entity] = enemyHealth;
            }).WithReadOnly(localToWorldData).Schedule();
        }
    }
}