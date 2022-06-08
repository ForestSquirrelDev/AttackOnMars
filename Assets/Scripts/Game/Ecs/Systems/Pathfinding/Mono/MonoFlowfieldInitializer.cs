using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems.Pathfinding.Mono {
    // Flowfield step 0: call initializer from monobehaviour class.
    public class MonoFlowfieldInitializer : MonoBehaviour {
        [SerializeField] private Transform _terrainTransform;
        
        private World World => World.DefaultGameObjectInjectionWorld;
        private FlowfieldManagerSystem _flowfieldManagerSystem => World.GetOrCreateSystem<FlowfieldManagerSystem>();

        private async void Awake() {
            await _flowfieldManagerSystem.Awake();
            _flowfieldManagerSystem.Init(_terrainTransform);
        }
    }
}