using Game.Ecs.Systems.Buildings;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace UI {
    public class HumanBaseHealthDisplayer : MonoBehaviour {
        [SerializeField] private TMP_Text _healthPercentage;
        [SerializeField] private Gradient _gradient;
        
        private HumanBaseHealthControllerSystem _healthController;

        private void Awake() {
            _healthController = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<HumanBaseHealthControllerSystem>();
        }

        private void Update() {
            var healthNormalized = (float)_healthController.CurrentHealth / _healthController.Config.MaxHealth;
            var color = _gradient.Evaluate(1 - healthNormalized);
            var healthPercents = Mathf.FloorToInt(healthNormalized * 100f);
            var text = $"{healthPercents.ToString()}%";
            _healthPercentage.text = text;
            _healthPercentage.color = color;
        }
    }
}