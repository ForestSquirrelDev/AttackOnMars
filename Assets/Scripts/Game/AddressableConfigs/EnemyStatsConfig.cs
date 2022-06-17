using UnityEngine;

namespace Game.AddressableConfigs {
    [CreateAssetMenu(menuName = "GameConfigs/Default Enemy Stats Config")]
    public class EnemyStatsConfig : ScriptableObject {
        public float XZMoveSpeed = 0.1f;
        public float YMoveSpeed = 0.2f;
        public float RotationSpeed = 10f;
    }
}