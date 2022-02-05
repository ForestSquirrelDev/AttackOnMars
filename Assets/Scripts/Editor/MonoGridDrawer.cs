using System;
using System.Collections.Generic;
using EasyButtons;
using Game;
using UnityEngine;
using Utils;

namespace Editor {
    public class MonoGridDrawer : MonoBehaviour {
        private GridManager manager;
        private int width, height;
        private float cellSize;
        private Vector3 parentPos;
        private List<Tuple<Vector3, Vector3>> linePositions = new();
        
        private void OnDrawGizmos() {
            manager ??= GetComponent<GridManager>();
            if (!BuildingGrid.Inited)
                manager.InitGrid();
            if (manager.Width != width || manager.Height != height || manager.CellSize != cellSize || manager.transform.position != parentPos) {
                UpdateGrid();
            }
            DrawGrid();
        }

        private void OnValidate() {
            manager ??= GetComponent<GridManager>();
            manager.InitGrid();
            UpdateGrid();
        }

        [Button]
        private void UpdateGrid() {
            width = manager.Width;
            height = manager.Height;
            cellSize = manager.CellSize;
            parentPos = manager.transform.position;
            linePositions.Clear();
            
            for (int x = 0; x < manager.Width; x++) {
                for (int z = 0; z < manager.Height; z++) {
                    Vector3 worldPos = BuildingGrid.GridToWorld(new Vector3(x, 0, z).RoundToVector2IntXZ());
                    Vector3 targetZ = new Vector3(worldPos.x, worldPos.y, worldPos.z + manager.CellSize);
                    targetZ.y = manager.Terrain.SampleHeight(targetZ);
                    Vector3 targetX = new Vector3(worldPos.x + manager.CellSize, worldPos.y, worldPos.z);
                    targetX.y = manager.Terrain.SampleHeight(targetX);
                    linePositions.Add(new Tuple<Vector3, Vector3>(worldPos, targetZ));
                    linePositions.Add(new Tuple<Vector3, Vector3>(worldPos, targetX));
                }
            }
            
            Vector3 rightBottomCorner = BuildingGrid.GridToWorld(new Vector3(width, 0, 0).RoundToVector2IntXZ());
            Vector3 rightTopCorner = BuildingGrid.GridToWorld(new Vector3(width, 0, height).RoundToVector2IntXZ());
            Vector3 leftTopCorner = BuildingGrid.GridToWorld(new Vector3(0, 0, height).RoundToVector2IntXZ());
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