using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Tags;
using Unity.Entities;

namespace Game.Ecs.Systems.Spawners {
    public partial class SetSucceededToWinStateSystem : SystemBase {
        protected override void OnUpdate() {
            if (HasSingleton<MainHumanBaseSingletonComponent>()) return;
            
            Entities.WithAll<Tag_Enemy>().ForEach((ref EnemyStateComponent state) => {
                if (state.Value == EnemyState.SucceededToWin) return;
                state.Value = EnemyState.SucceededToWin;
            }).ScheduleParallel();
        }
    }
}