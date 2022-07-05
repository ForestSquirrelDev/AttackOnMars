using Game.AddressableConfigs;
using Game.Ecs.Components;
using Game.Ecs.Components.Buildings;
using Unity.Collections;
using Unity.Entities;

namespace Game.Ecs.Systems.Buildings {
    public partial class HumanBaseHealthControllerSystem : SystemBase {
        public HumanBaseConfig Config { get; private set; }
        public int CurrentHealth { get; private set; }

        private NativeArray<int> _currentHealthOut;

        public void InjectConfigs(HumanBaseConfig config) {
            Config = config;
        }

        protected override void OnCreate() {
            _currentHealthOut = new NativeArray<int>(1, Allocator.Persistent);
        }

        public void Init() {
            int maxHealth = Config.MaxHealth;
            CurrentHealth = maxHealth;
            
            Entities.WithAll<Tag_MainHumanBase>().ForEach((ref BuildingHealthComponent health) => {
                health.CurrentHealth = maxHealth;
            }).Run();
        }

        protected override void OnUpdate() {
            var currentHealthOut = _currentHealthOut;
            CurrentHealth = currentHealthOut[0];
            
            Dependency = Entities.WithAll<Tag_MainHumanBase>().ForEach((in BuildingHealthComponent healthIn) => {
                currentHealthOut[0] = healthIn.CurrentHealth;
            }).Schedule(Dependency);
        }

        protected override void OnDestroy() {
            _currentHealthOut.Dispose();
        }
    }
}