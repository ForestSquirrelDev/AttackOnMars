using Unity.Entities;

namespace Game.Ecs.Systems.Buildings {
    public class MonoInitializeHumanBaseSystem : GameManagerBase {
        public override void OnAwake() {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<HumanBaseHealthControllerSystem>().Init();
        }
    }
}