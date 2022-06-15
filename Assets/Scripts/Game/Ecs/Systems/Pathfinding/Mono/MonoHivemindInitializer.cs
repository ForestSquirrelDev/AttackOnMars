using Unity.Entities;

namespace Game.Ecs.Systems.Pathfinding.Mono {
    public class MonoHivemindInitializer : GameManagerBase {
        private World _world => World.DefaultGameObjectInjectionWorld;

        public override void OnAwake() {
            _world.GetOrCreateSystem<HivemindSystem>().Init();
        }
    }
}