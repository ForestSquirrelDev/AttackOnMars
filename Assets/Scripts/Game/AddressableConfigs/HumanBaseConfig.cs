using UnityEngine;

namespace Game.AddressableConfigs {
    [CreateAssetMenu(menuName = "GameConfigs/HumanBaseConfig")]
    public class HumanBaseConfig : ScriptableObject {
        public int MaxHealth = 50000;
    }
}