using UnityEngine;

namespace Game.AddressableConfigs {
    [CreateAssetMenu(menuName = "GameConfigs/Enemies Spawner Config")]
    public class EnemiesSpawnerConfig : ScriptableObject {
        public int EnemiesCount = 50;
        public int CountPerFrame = 10;
    }
}