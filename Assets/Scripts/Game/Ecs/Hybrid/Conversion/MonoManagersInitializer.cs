using System.Collections.Generic;
using Game.Ecs.Hybrid.Conversion;
using UnityEngine;

namespace Game.Ecs.Systems {
    public class MonoManagersInitializer : MonoBehaviour {
        [SerializeField] private List<GameObjectsConverterBase> _convertersOrdered;
        [SerializeField] private List<SystemInitializerBase> _initializersOrdered;

        private void Awake() {
            foreach (var converter in GetComponentsInChildren<GameObjectsConverterBase>()) {
                converter.Convert();
            }
            foreach (var initializer in GetComponentsInChildren<SystemInitializerBase>()) {
                initializer.OnAwake();
            }
        }
    }
}