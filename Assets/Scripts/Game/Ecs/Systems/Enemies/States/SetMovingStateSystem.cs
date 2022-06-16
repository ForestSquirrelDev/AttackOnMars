using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Tags;
using Unity.Entities;

namespace Game.Ecs.Systems.Spawners {
    public partial class SetMovingStateSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithAll<Tag_Enemy>().ForEach((ref EnemyStateComponent enemyState) => {
                if (enemyState.Value != EnemyState.Entry) return;
                enemyState.Value = EnemyState.Moving;
            }).Schedule();
        }
    }
}