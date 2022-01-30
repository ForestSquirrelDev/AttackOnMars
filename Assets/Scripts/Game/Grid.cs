using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Game {
    public class Grid {
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        
        private int width;
        private int height;
        private float cellSize;
        private Transform parent;
        private List<GridObject>[,] objectsGrid;
        private int[,] cells;

        public Grid(int width, int height, float cellSize, Transform parent) {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.parent = parent;

            cells = new int[width, height];
            objectsGrid = new List<GridObject>[width, height];
        }

        public Vector3 GridToWorld(Vector3 cell) {
            return parent.TransformPoint(cell * cellSize);
        }

        public Vector3 GridToWorldCentered(Vector3 cell) {
            return parent.TransformPoint(cell.CenterXZ() * cellSize);
        }
        
        public Vector3 WorldToGridFloored(Vector3 world) {
            return (parent.InverseTransformPoint(world) / cellSize).FloorToInt();
        }

        public GameObject InstantiateOnGrid(Vector3 inputWorldPos, GameObject prefab, Quaternion rotation, Transform parent) {
            Vector3Int tile = WorldToGridFloored(inputWorldPos).RoundToVector3Int();
            if (tile.x < 0 || tile.x >= width || tile.z < 0 || tile.z >= height) {
                Debug.LogWarning("Can't instantiate: index was out of range");
                return null;
            }
            if (TileIsOccupied(tile)) {
                Debug.Log("Tile is occupied");
                return null;
            }
            if (prefab == null) {
                Debug.LogWarning("Can't instantiate: missing prefab reference");
                return null;
            }
            Vector3 worldPosition = GridToWorldCentered(tile);
            worldPosition.y = inputWorldPos.y;
            GameObject obj = Object.Instantiate(prefab, worldPosition, rotation, parent);
            if (!obj.TryGetComponent(out GridObject gridObject)) {
                Object.Destroy(obj);
                Debug.LogWarning("Object was destroyed due to missing GridObject component.");
                return null;
            }
            AddGridObject(tile, gridObject);
            return obj;
        }

        public bool TileIsOccupied(Vector3Int tile) {
            return objectsGrid[tile.x, tile.z] != null && objectsGrid[tile.x, tile.z].Count > 0;
        }

        private void AddGridObject(Vector3Int tile, GridObject gridObject) {
            objectsGrid[tile.x, tile.z] ??= new List<GridObject>();
            objectsGrid[tile.x, tile.z].Add(gridObject);
        }
    }
}