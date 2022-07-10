using Game.AddressableConfigs;
using Game.Ecs.Components;
using Game.Ecs.Components.Buildings;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems.Buildings {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class HumanBaseHealthObserverSystem : SystemBase {
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }
        public bool HumanBaseIsAlive => ShouldRunSystem();
        
        private HumanBaseConfig _config;
        private NativeArray<int> _currentHealthOut;

        protected override void OnCreate() {
            var humanBaseQuery = EntityManager.CreateEntityQuery(new EntityQueryDesc {
                All = new[] { ComponentType.ReadOnly<MainHumanBaseSingletonComponent>() }
            });
            RequireForUpdate(humanBaseQuery);
            _config = AddressablesLoader.Get<HumanBaseConfig>(AddressablesConsts.DefaultHumanBaseConfig);
            _currentHealthOut = new NativeArray<int>(1, Allocator.Persistent);
        }

        public void Init() {
            MaxHealth = _config.MaxHealth;
            CurrentHealth = MaxHealth;
        }

        protected override void OnUpdate() {
            var currentHealthOut = _currentHealthOut;
            CurrentHealth = currentHealthOut[0];
            
            Dependency = Entities.WithAll<Tag_MainHumanBase>().ForEach((in BuildingHealthComponent healthIn) => {
                currentHealthOut[0] = healthIn.Value;
            }).Schedule(Dependency);
        }

        protected override void OnDestroy() {
            _currentHealthOut.Dispose();
        }
    }
}