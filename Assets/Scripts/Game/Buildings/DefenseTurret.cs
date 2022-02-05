using UnityEngine;

namespace Game.Buildings {
    public class DefenseTurret : Building {
        public void Awake() {
            foreach (var VARIABLE in positionsInGrid) {
                Debug.Log(VARIABLE);
            }
        }
    }
}