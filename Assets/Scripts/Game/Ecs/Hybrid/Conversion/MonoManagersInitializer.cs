using System.Collections.Generic;
using Game.Ecs.Hybrid.Conversion;
using UnityEngine;

namespace Game.Ecs.Systems {
    public class MonoManagersInitializer : MonoBehaviour {
        [SerializeField] private List<GameObjectsConverterBase> _convertersOrdered;
        [SerializeField] private List<GameManagerBase> _initializersOrdered;

        private void Awake() {
            foreach (var converter in _convertersOrdered) {
                converter.Convert();
            }
            foreach (var initializer in _initializersOrdered) {
                initializer.OnAwake();
            }
        }
    }
}