using System;
using System.Collections.Generic;
using Game;
using UnityEditor;
using UnityEngine;
using Grid = Game.Grid;

namespace Editor {
    [CustomEditor(typeof(GridManager))]
    public class EditorGrid : UnityEditor.Editor {
        private Grid grid;
        private int width, height;
        private float cellSize;
        private Vector3 parentPos;
        private List<Tuple<Vector3, Vector3>> linePositions = new();

        private void OnSceneGUI() {
            DrawGrid();
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            GridManager manager = target as GridManager;
            if (grid == null || manager.width != width || manager.height != height || manager.cellSize != cellSize || manager.transform.position != parentPos) {
                UpdateGrid(manager);
            }
        }

        private void DrawGrid() {
            foreach (var direction in linePositions) {
                Handles.DrawLine(direction.Item1, direction.Item2);
            }
        }

        private void UpdateGrid(GridManager manager) {
            width = manager.width;
            height = manager.height;
            cellSize = manager.cellSize;
            parentPos = manager.transform.position;
            grid = new Grid(manager.width, manager.height, manager.cellSize, manager.transform);
            linePositions.Clear();
            for (int x = 0; x < grid.Width; x++) {
                for (int z = 0; z < grid.Height; z++) {
                    Vector3 worldPos = grid.GridCellToWorld(new Vector3(x, 0, z));
                    linePositions.Add(new Tuple<Vector3, Vector3>(worldPos, new Vector3(worldPos.x, worldPos.y, worldPos.z + manager.cellSize)));
                    linePositions.Add(new Tuple<Vector3, Vector3>(worldPos, new Vector3(worldPos.x + manager.cellSize, worldPos.y, worldPos.z)));
                }
            }
            Vector3 rightBottomCorner = grid.GridCellToWorld(new Vector3(width, 0, 0));
            Vector3 rightTopCorner = grid.GridCellToWorld(new Vector3(width, 0, height));
            Vector3 leftTopCorner = grid.GridCellToWorld(new Vector3(0, 0, height));
            linePositions.Add(new Tuple<Vector3, Vector3>(rightBottomCorner, rightTopCorner));
            linePositions.Add(new Tuple<Vector3, Vector3>(leftTopCorner, rightTopCorner));
        }
    }
}