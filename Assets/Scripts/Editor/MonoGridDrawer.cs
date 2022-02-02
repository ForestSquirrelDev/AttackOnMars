using System;
using System.Collections.Generic;
using Game;
using UnityEngine;
using Utils;
using Grid = Game.Grid;

namespace Editor {
    public class MonoGridDrawer : MonoBehaviour {
        private GridManager manager;
        private Grid grid;
        private int width, height;
        private float cellSize;
        private Vector3 parentPos;
        private List<Tuple<Vector3, Vector3>> linePositions = new();
        
        private void OnDrawGizmos() {
            manager ??= GetComponent<GridManager>();
            if (grid == null || manager.Width != width || manager.Height != height || manager.CellSize != cellSize || manager.transform.position != parentPos) {
                UpdateGrid(manager);
            }
            DrawGrid();
        }
        
        private void UpdateGrid(GridManager manager) {
            width = manager.Width;
            height = manager.Height;
            cellSize = manager.CellSize;
            parentPos = manager.transform.position;
            grid = new Grid(manager.Width, manager.Height, manager.CellSize, manager.transform, manager.Terrain);
            linePositions.Clear();
            for (int x = 0; x < grid.Width; x++) {
                for (int z = 0; z < grid.Height; z++) {
                    Vector3 worldPos = grid.GridToWorld(new Vector3(x, 0, z).RoundToVector2IntXZ());
                    Vector3 targetZ = new Vector3(worldPos.x, worldPos.y, worldPos.z + manager.CellSize);
                    targetZ.y = manager.Terrain.SampleHeight(targetZ);
                    Vector3 targetX = new Vector3(worldPos.x + manager.CellSize, worldPos.y, worldPos.z);
                    targetX.y = manager.Terrain.SampleHeight(targetX);
                    linePositions.Add(new Tuple<Vector3, Vector3>(worldPos, targetZ));
                    linePositions.Add(new Tuple<Vector3, Vector3>(worldPos, targetX));
                }
            }
            Vector3 rightBottomCorner = grid.GridToWorld(new Vector3(width, 0, 0).RoundToVector2IntXZ());
            Vector3 rightTopCorner = grid.GridToWorld(new Vector3(width, 0, height).RoundToVector2IntXZ());
            Vector3 leftTopCorner = grid.GridToWorld(new Vector3(0, 0, height).RoundToVector2IntXZ());
            linePositions.Add(new Tuple<Vector3, Vector3>(rightBottomCorner, rightTopCorner));
            linePositions.Add(new Tuple<Vector3, Vector3>(leftTopCorner, rightTopCorner));
        }
        
        private void DrawGrid() {
            foreach (var direction in linePositions) {
                Gizmos.DrawLine(direction.Item1, direction.Item2);
            }
        }
    }
}