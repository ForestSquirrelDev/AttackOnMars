using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.AddressableConfigs {
    [CreateAssetMenu(menuName = "GameConfigs/Default Enemy Stats Config")]
    public class EnemyStatsConfig : ScriptableObject {
        [Header("Movement")]
        public float XZMoveSpeed = 0.1f;
        public float XZRandomSpeedFactor = 0.1f;
        public float YMoveSpeed = 0.2f;
        public float RotationSpeed = 10f;

        [Header("Enemy base detection")]
        public float RaycastHeight = 3f;
        public float RaycastLength = 3;
        public float RaycastCooldown = 0.5f;
        public bool CheckNeighbourCells = true;
        
        [Header("Attack")]
        public float AttackCooldown = 1f;
        public float3 StartRaycastOffset = new float3(0f, 0f, 3f);
        public float AttackRaycastLength = 3f;
        public int Damage = 1000;

        [Header("Misc")]
        public float Health = 500;
        public int GridDirectionUpdateSkipCount = 10;
    }
}