using Unity.Mathematics;
using UnityEngine;

namespace Game.AddressableConfigs {
    [CreateAssetMenu(menuName = "GameConfigs/Default Enemy Stats Config")]
    public class EnemyStatsConfig : ScriptableObject {
        public float XZMoveSpeed = 0.1f;
        public float YMoveSpeed = 0.2f;
        public float RotationSpeed = 10f;

        [Header("Enemy base detection")]
        public float RaycastHeight = 3f;
        public float RAycastLength = 3;
        
        [Header("Attack")]
        public float AttackCooldown = 1f;
        public float3 BoxCastOffset = new float3(0f, 0f, 0.5f);
        public float3 BoxCastSize = new float3(0.5f, 0.5f, 0.5f);
        public float BoxCastMaxDistance = 2f;
        public int Damage = 1000;
    }
}