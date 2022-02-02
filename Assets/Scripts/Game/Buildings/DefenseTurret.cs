using System.Collections.Generic;
using UnityEngine;

namespace Game.Buildings {
    public class DefenseTurret : MonoBehaviour, IBuilding {
        public BuildingData BuildingData => buildingData;
        public HashSet<Vector2Int> positionsInGrid { get; set; } = new();
        public Vector2Int PositionInGrid { get; set; }
        
        [SerializeField] private MeshFilter positioningQuad;
        [SerializeField] private BuildingData buildingData;

        private List<Vector3> vertices = new();

        public List<Vector3> GetPositioningQuadCorners() {
            if (vertices.Count == 0)
                positioningQuad.mesh.GetVertices(vertices);
            for (int i = 0; i < vertices.Count; i++) {
                vertices[i] = transform.TransformPoint(vertices[i]);
            }
            return vertices;
        }

        

        private void Awake() {
            
        }
    }
}