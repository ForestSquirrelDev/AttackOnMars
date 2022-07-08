using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Tags;
using Unity.Entities;

namespace Game.Ecs.Systems.Spawners {
    [UpdateAfter(typeof(DestroyDeadEnemiesSystem))]
    public partial class ClearTurretsTargetSystem : SystemBase {
		protected override void OnUpdate() {
            var healthData = GetComponentDataFromEntity<EnemyHealthComponent>(true);
            Entities.WithAll<Tag_Turret>().ForEach((ref CurrentTurretTargetComponent currentTarget) => {
                var isValidEntity = healthData.HasComponent(currentTarget.Entity);
                if (!isValidEntity) {
                    currentTarget.Entity = Entity.Null;
                    currentTarget.Ltw = default;
                }
            }).WithReadOnly(healthData).Schedule();
        }
    }
}