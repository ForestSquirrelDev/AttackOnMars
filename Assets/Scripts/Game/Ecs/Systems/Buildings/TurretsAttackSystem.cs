using Game.AddressableConfigs;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Ecs.Systems.Spawners {
    public partial class TurretsAttackSystem : SystemBase {
        private TurretsConfig _turretsConfig;

        protected override void OnCreate() {
            _turretsConfig = ConfigsLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
        }

        protected override void OnUpdate() {
            Entities.WithAll<Tag_Turret>().ForEach((in Rotation rotation) => {

            }).Schedule();
        }
    }
}