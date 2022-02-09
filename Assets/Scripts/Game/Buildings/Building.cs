using System.Collections.Generic;
using UnityEngine;

namespace Game.Buildings {
    public abstract class Building : MonoBehaviour {
        public HashSet<Vector2Int> positionsInGrid { get; set; } = new HashSet<Vector2Int>();
        [SerializeField] private PositioningQuad positioningQuad;

        public void Init() {
            positioningQuad.Init();
            positionsInGrid = new HashSet<Vector2Int>(positioningQuad.GetOccupiedGlobalGridTiles());
        }
    }
}
