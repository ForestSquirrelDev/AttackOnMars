using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Game.Buildings {
    public abstract class Building : MonoBehaviour {
        public HashSet<Vector2Int> positionsInGrid { get; set; } = new();
        [SerializeField] private PositioningQuad positioningQuad;

        public void Init() {
            positioningQuad.Init();
            positionsInGrid = new HashSet<Vector2Int>(positioningQuad.GetOccupiedGlobalGridTiles());
        }

        public ReadOnlyCollection<Vector3> GetPositioningQuadCorners() => positioningQuad.GetWorldCorners();
    }
}
