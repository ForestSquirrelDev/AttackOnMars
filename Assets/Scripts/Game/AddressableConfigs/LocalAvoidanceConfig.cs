using UnityEngine;

namespace Game.AddressableConfigs {
    [CreateAssetMenu(menuName = "GameConfigs/LocalAvoidanceConfig")]
    public class LocalAvoidanceConfig : ScriptableObject {
        public float MaxDistanceSquared = 150;
        public float TunableSigmoidK = -0.7f;
        public float MaxVectorLength = 0.5f;
        public Vector2Int FramesSkipRange = new Vector2Int(20, 100);
    }
}