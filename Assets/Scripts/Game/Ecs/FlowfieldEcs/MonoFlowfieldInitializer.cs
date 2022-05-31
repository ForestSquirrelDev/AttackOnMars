using System;
using EasyButtons;
using Game.Ecs.Flowfield.Systems;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Flowfield {
    // Flowfield step 0: call initializer from monobehaviour class.
    public class MonoFlowfieldInitializer : MonoBehaviour {
        [SerializeField] private Transform _terrainTransform;
        
        private World World => World.DefaultGameObjectInjectionWorld;

        private void Awake() {
            Init();
        }
        
        private void Init() {
            var flowfieldManagerSystem = World.GetOrCreateSystem<FlowfieldManagerSystem>();
            flowfieldManagerSystem.Awake(_terrainTransform);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Delete)) {
                Init();
            }
        }
    }
}