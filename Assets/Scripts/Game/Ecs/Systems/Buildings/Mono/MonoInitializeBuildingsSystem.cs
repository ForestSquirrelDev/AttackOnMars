using Unity.Entities;

namespace Game.Ecs.Systems.Buildings {
    public class MonoInitializeBuildingsSystem : GameManagerBase {
        public override void OnAwake() {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<HumanBaseHealthObserverSystem>().Init();
        }
    }
}