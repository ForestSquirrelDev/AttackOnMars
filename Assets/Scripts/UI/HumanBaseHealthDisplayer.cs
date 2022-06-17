using Game.Ecs.Systems.Buildings;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace UI {
    public class HumanBaseHealthDisplayer : MonoBehaviour {
        [SerializeField] private TMP_Text _healthPercentage;

        private HumanBaseHealthControllerSystem _healthController;

        private void Awake() {
            _healthController = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<HumanBaseHealthControllerSystem>();
        }

        private void Update() {
            var healthNormalized = (float)_healthController.CurrentHealth / _healthController.Config.MaxHealth;
            var healthPercents = healthNormalized * 100;
            var text = $"{healthPercents.ToString()}%";
            _healthPercentage.text = text;
        }
    }
}