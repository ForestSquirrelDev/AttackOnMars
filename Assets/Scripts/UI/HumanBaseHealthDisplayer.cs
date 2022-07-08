using Game.Ecs.Systems.Buildings;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace UI {
    public class HumanBaseHealthDisplayer : MonoBehaviour {
        [SerializeField] private TMP_Text _healthPercentage;
        [SerializeField] private Gradient _gradient;
        
        private HumanBaseHealthObserverSystem _healthObserver;

        private void Awake() {
            _healthObserver = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<HumanBaseHealthObserverSystem>();
        }

        private void Update() {
            var healthNormalized = (float)_healthObserver.CurrentHealth / _healthObserver.MaxHealth;
            var color = _gradient.Evaluate(1 - healthNormalized);
            var healthPercents = Mathf.FloorToInt(healthNormalized * 100f);
            var text = $"{healthPercents.ToString()}%";
            _healthPercentage.text = text;
            _healthPercentage.color = color;
        }
    }
}