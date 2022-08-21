using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems.Spawners {
    public class MonoSpawnEnemiesSystemInitializer : SystemInitializerBase {
        [SerializeField] private Terrain _terrain;

        public override void OnAwake() {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SpawnEnemiesSystem>().Init(_terrain);
        }
    }
}