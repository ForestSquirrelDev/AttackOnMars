using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Ecs.Systems.Spawners {
    public partial class UpdateCurrentTurretsTargetsSystem : SystemBase {
        private TurretsConfig _turretsConfig;

        protected override void OnCreate() {
            _turretsConfig = ConfigsLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
        }

        protected override void OnUpdate() {
            var localToWorldData = GetComponentDataFromEntity<LocalToWorld>();
            var maxRadius = _turretsConfig.EffectiveRadius;
            
            Dependency = Entities.WithAll<Tag_Turret>().ForEach((ref CurrentTurretTargetComponent currentEnemyTarget, in Entity turretEntity) => {
                if (currentEnemyTarget.Entity == Entity.Null) return;
                
                var enemyLtw = localToWorldData[currentEnemyTarget.Entity];
                var turretLtw = localToWorldData[turretEntity];
                if (math.distance(enemyLtw.Position, turretLtw.Position) > maxRadius) {
                    currentEnemyTarget.Entity = Entity.Null;
                    currentEnemyTarget.Ltw = default;
                } else {
                    currentEnemyTarget.Ltw = enemyLtw;
                }
            }).Schedule(Dependency);
        }
    }
}
