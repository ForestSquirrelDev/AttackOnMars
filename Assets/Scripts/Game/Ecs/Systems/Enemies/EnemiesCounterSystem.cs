using Game.Ecs.Components.Tags;
using Unity.Entities;

namespace Game.Ecs.Systems.Spawners {
    public partial class EnemiesCounterSystem : SystemBase {
        public int Counter { get; private set; }
        
        protected override void OnUpdate() {
            int counter = 0;
            Entities.WithAll<Tag_Enemy>().ForEach(() => {
                counter++;
            }).Run();
            Counter = counter;
        }
    }
}