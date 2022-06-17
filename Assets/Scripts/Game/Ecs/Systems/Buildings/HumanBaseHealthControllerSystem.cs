using Game.AddressableConfigs;
using Game.Ecs.Components;
using Game.Ecs.Components.Buildings;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems.Buildings {
    public partial class HumanBaseHealthControllerSystem : SystemBase {
        public HumanBaseConfig Config { get; private set; }
        public int CurrentHealth { get; private set; }

        public void InjectConfigs(HumanBaseConfig config) {
            Config = config;
        }

        public void Init() {
            int maxHealth = Config.MaxHealth;
            CurrentHealth = maxHealth;
            
            Entities.WithAll<Tag_MainHumanBase>().ForEach((ref HealthComponent health) => {
                health.CurrentHealth = maxHealth;
            }).Run();
        }

        protected override void OnUpdate() {
            int currentHealthOut = 0;

            Entities.WithAll<Tag_MainHumanBase>().ForEach((in HealthComponent healthIn) => {
                currentHealthOut = healthIn.CurrentHealth;
            }).Run();

            if (Input.GetKeyDown(KeyCode.U)) {
                Entities.WithAll<Tag_MainHumanBase>().ForEach((ref HealthComponent health) => {
                    health.CurrentHealth -= 1000;
                }).Run();
            }
            
            CurrentHealth = currentHealthOut;
        }
    }
}