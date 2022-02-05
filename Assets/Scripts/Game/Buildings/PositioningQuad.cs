using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Utils;

namespace Game {
    public class PositioningQuad : MonoBehaviour {
        [SerializeField] private Transform gridOrigin;
        [SerializeField] private MeshFilter meshFilter;

        private Matrix4x4 gridToWorld;
        private PositioningGrid positioningGrid;
        private List<Vector3> vertices = new();
        private List<Vector2Int> localGrid = new();
        private List<Vector3> worldGrid = new();
        private List<Vector2Int> globalGrid = new();

        public void Init() {
            // we are constructing matrix because this grid needs to be independent from parent transforms, i.e. we need pure grid with tiles of unit size
            // because quad is rotated by 90 degrees on x axis, we need to set "up" vector as "forward" and vice versa
            // x
            gridToWorld.SetColumn(0, gridOrigin.right);
            // y
            gridToWorld.SetColumn(1, gridOrigin.forward);
            // z
            gridToWorld.SetColumn(2, gridOrigin.up);
            // position
            gridToWorld.SetColumn(3, gridOrigin.position);
            InitGrid();
        }
        
        public ReadOnlyCollection<Vector3> GetWorldCorners() {
            vertices.Clear();
            meshFilter.mesh.GetVertices(vertices);
            for (int i = 0; i < vertices.Count; i++) {
                vertices[i] = vertices[i].ToVector3XZ();
                vertices[i] = transform.TransformPoint(vertices[i]);
            }
            return vertices.AsReadOnly();
        }

        public List<Vector2Int> GetOccupiedGlobalGridTiles() {
            positioningGrid.GetGrid(localGrid);
            FillWorldGrid();
            FillGlobalGrid();
            return globalGrid;
        }
        
        private void InitGrid() {
            Vector2Int size = CalculateGridSize();
            positioningGrid = new PositioningGrid(size.x, size.y);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            foreach (var VARIABLE in worldGrid) {
                Gizmos.DrawSphere(VARIABLE, 0.2f);
            }
        }

        private void FillWorldGrid() {
            worldGrid.Clear();
            foreach (var tile in localGrid) {
                Vector3 world = gridToWorld.MultiplyPoint3x4(tile.ToVector3XZ() * BuildingGrid.CellSize);
                worldGrid.Add(world);
            }
        }
        
        private void FillGlobalGrid() {
            globalGrid.Clear();
            foreach (var tile in worldGrid) {
                globalGrid.Add(BuildingGrid.WorldToGridFloored(tile));
            }
        }
        
        private Vector2Int CalculateGridSize() {
            if (vertices.Count == 0)
                meshFilter.mesh.GetVertices(vertices);
            Vector3 leftBottomCorner = vertices[0];
            Vector3 leftTopCorner = vertices[2];
            Vector3 rightBottomCorner = vertices[1];

            leftBottomCorner = gridOrigin.TransformPoint(leftBottomCorner);
            leftTopCorner = gridOrigin.TransformPoint(leftTopCorner);
            rightBottomCorner = gridOrigin.TransformPoint(rightBottomCorner);

            Vector2Int leftBottomToGlobalGrid = BuildingGrid.WorldToGridFloored(leftBottomCorner);
            Vector2Int leftTopToGlobalGrid = BuildingGrid.WorldToGridFloored(leftTopCorner);
            Vector2Int rightBottomToGlobalGrid = BuildingGrid.WorldToGridFloored(rightBottomCorner);
            
            int width = Mathf.Abs(rightBottomToGlobalGrid.x - leftBottomToGlobalGrid.x);
            int height = Mathf.Abs(leftTopToGlobalGrid.y - leftBottomToGlobalGrid.y);
            return new Vector2Int(width, height);
        }
    }
}