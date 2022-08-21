using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems.Pathfinding.Mono {
    // Flowfield step 0: call initializer from monobehaviour class.
    public class MonoFlowfieldInitializer : SystemInitializerBase {
        [SerializeField] private Transform _terrainTransform;
        
        private World World => World.DefaultGameObjectInjectionWorld;
        private FlowfieldManagerSystem _flowfieldManagerSystem => World.GetOrCreateSystem<FlowfieldManagerSystem>();

        public override void OnAwake() {
            _flowfieldManagerSystem.Init(_terrainTransform);
        }
    }
}