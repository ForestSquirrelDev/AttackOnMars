using Game.Ecs.Systems.Spawners;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace UI {
    public class EnemiesCounterDisplayer : MonoBehaviour {
        [SerializeField] private TMP_Text _counterText;
        
        private EnemiesCounterSystem _counterSystem;

        private void Awake() {
            _counterSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EnemiesCounterSystem>();
        }

        private void Update() {
            _counterText.text = $"Enemies count: {_counterSystem.Counter.ToString()}";
        }
    }
}