using UnityEngine;

namespace Game.Buildings {
    [CreateAssetMenu(menuName = "Configs/Building Data")]
    public class BuildingData : ScriptableObject {
        [Range(0, 5)]
        public int gridWidth = 1, gridHeight = 1;
    }
}
