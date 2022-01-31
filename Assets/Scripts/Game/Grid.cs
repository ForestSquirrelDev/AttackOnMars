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
        private Dictionary<Vector2Int, GridObject> gridObjects = new();
        private int[,] cells;

        public Grid(int width, int height, float cellSize, Transform parent) {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.parent = parent;

            cells = new int[width, height];
        }

        public Vector3 GridToWorld(Vector2Int cell) {
            return parent.TransformPoint(cell.ToVector3XZ() * cellSize);
        }

        public Vector3 GridToWorldCentered(Vector2Int cell) {
            return parent.TransformPoint(cell.ToVector3XZ().CenterXZ() * cellSize);
        }
        
        public Vector2Int WorldToGridFloored(Vector3 world) {
            return (parent.InverseTransformPoint(world) / cellSize).FloorToVector2IntXZ();
        }

        public GameObject InstantiateOnGrid(Vector3 inWorldPos, GameObject prefab, Quaternion rotation, Transform parent) {
            Vector2Int tile = WorldToGridFloored(inWorldPos);
            if (tile.x < 0 || tile.x >= width || tile.y < 0 || tile.y >= height) {
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
            Vector3 outWorldPos = GridToWorldCentered(tile);
            outWorldPos.y = inWorldPos.y;
            GameObject obj = Object.Instantiate(prefab, outWorldPos, rotation, parent);
            if (!obj.TryGetComponent(out GridObject gridObject)) {
                Object.Destroy(obj);
                Debug.LogWarning("Object was destroyed due to missing GridObject component.");
                return null;
            }
            gridObject.Position = tile;
            gridObjects.Add(tile, gridObject);
            return obj;
        }

        public bool TileIsOccupied(Vector2Int tile) {
            return gridObjects.ContainsKey(tile);
        }
    }
}