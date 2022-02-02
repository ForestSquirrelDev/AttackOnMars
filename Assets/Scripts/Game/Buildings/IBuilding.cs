using System.Collections.Generic;
using UnityEngine;

namespace Game.Buildings {
    public interface IBuilding {
        public BuildingData BuildingData { get; }
        public HashSet<Vector2Int> positionsInGrid { get; set; }

        public List<Vector3> GetPositioningQuadCorners();
    }
}
