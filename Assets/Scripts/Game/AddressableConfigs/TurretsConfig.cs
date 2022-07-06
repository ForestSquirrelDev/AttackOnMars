using UnityEngine;

namespace Game.AddressableConfigs {
    [CreateAssetMenu(menuName = "Game Configs/Turrets Config")]
    public class TurretsConfig : ScriptableObject {
        public float RadarFrequencySeconds = 1f;
        public float EffectiveRadius = 300f;
        public int AttacksPerUpdate = 10;
        public float Damage = 1f;
        public float RotationSpeed = 10f;
        
        [Range(0, 1f)]
        // tested against dot product between turret forward vector and direction to enemy
        public float AttackRotationError = 0.95f;
    }
}