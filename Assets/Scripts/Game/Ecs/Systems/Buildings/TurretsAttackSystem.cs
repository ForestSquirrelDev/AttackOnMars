using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Tags;
using Unity.Entities;

namespace Game.Ecs.Systems.Spawners {
    [UpdateAfter(typeof(TurretsRadarSystem))]
    public partial class TurretsAttackSystem : SystemBase {
        private TurretsConfig _turretsConfig;

        protected override void OnCreate() {
            _turretsConfig = ConfigsLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
        }

        protected override void OnUpdate() {
            var enemyHealthData = GetComponentDataFromEntity<EnemyHealthComponent>(false);
            var damage = _turretsConfig.Damage;
            var attacksPerUpdate = _turretsConfig.AttacksPerUpdate;
                
            Entities.WithAll<Tag_Turret>().ForEach((in RotatableTurretPartsReferenceComponent rotatable, in CurrentTurretTargetComponent currentEnemyTarget, in CurrentTurretStateComponent state) => {
                if (state.Value != TurretState.Attacking) return;
                var isValidEntity = enemyHealthData.HasComponent(currentEnemyTarget.Entity);
                if (!isValidEntity) return;
                
                var enemyHealth = enemyHealthData[currentEnemyTarget.Entity];
                for (int i = 0; i < attacksPerUpdate; i++) {
                    enemyHealth.Value -= damage;
                }
                enemyHealthData[currentEnemyTarget.Entity] = enemyHealth;
            }).Schedule();
        }
    }
}