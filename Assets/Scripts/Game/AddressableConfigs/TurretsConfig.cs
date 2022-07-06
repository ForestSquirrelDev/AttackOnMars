using UnityEngine;

namespace Game.AddressableConfigs {
    [CreateAssetMenu(menuName = "Game Configs/Turrets Config")]
    public class TurretsConfig : ScriptableObject {
        public float RadarFrequencySeconds = 1f;
        public float EffectiveRadius = 300f;
        public int AttacksPerUpdate = 10;
        public float Damage = 1f;
        public float RotationSpeed = 10f;
    }
}