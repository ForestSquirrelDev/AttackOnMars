using Unity.Mathematics;
using UnityEngine;

namespace Game.AddressableConfigs {
    [CreateAssetMenu(menuName = "Game Configs/Turrets Config")]
    public class TurretsConfig : ScriptableObject {
        public float RadarFrequencySeconds = 1f;
        public float EffectiveRadius = 300f;
        public int AttacksPerUpdate = 10;
        public float Damage = 1f;
        public float BaseRotationSpeed = 10f;
        public float BarrelMaxRotationSpeed = 10f;
        public float BarrelRotationSpeedIncreateStep = 0.1f;
        public float3 ParticlesOffset = new float3(0f, 0.466f, 1.85f);
        
        [Range(0, 1f)]
        // tested against dot product between turret forward vector and direction to enemy
        public float LookAtEnemyError = 0.95f;
    }
}