using System.Collections.Generic;
using Game.Buildings;
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
        private Terrain terrain;
        private Dictionary<HashSet<Vector2Int>, IBuilding> gridObjects = new();
        private List<Vector3> positioningCornersBuffer = new();
        private List<Vector2Int> occupiedTilesBuffer = new();
        private int[,] cells;

        public Grid(int width, int height, float cellSize, Transform parent, Terrain terrain) {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.parent = parent;
            this.terrain = terrain;

            cells = new int[width, height];
        }

        public Vector3 GridToWorld(Vector2Int cell) {
            Vector3 world = parent.TransformPoint(cell.ToVector3XZ() * cellSize);
            world.y = terrain.SampleHeight(world);
            return world;
        }

        public Vector3 GridToWorldCentered(Vector2Int cell) {
            Vector3 world = parent.TransformPoint(cell.ToVector3XZ().CenterXZ() * cellSize);
            world.y = terrain.SampleHeight(world);
            return world;
        }
        
        public Vector2Int WorldToGridFloored(Vector3 world) {
            return (parent.InverseTransformPoint(world) / cellSize).FloorToVector2IntXZ();
        }

        public GameObject InstantiateOnGrid(Vector3 inWorldPos, GameObject prefab, Quaternion rotation, Transform parent) {
            GameObject obj = Object.Instantiate(prefab, Vector3.zero, rotation, parent);
            if (!obj.TryGetComponent(out IBuilding building)) {
                Object.Destroy(obj);
                Debug.LogWarning("Object was destroyed due to missing building component.");
                return null;
            }
            positioningCornersBuffer.Clear();
            occupiedTilesBuffer.Clear();
            positioningCornersBuffer.AddRange(building.GetPositioningQuadCorners());
            foreach (Vector3 corner in positioningCornersBuffer) {
                occupiedTilesBuffer.Add(WorldToGridFloored(corner));
            }
            building.positionsInGrid = new HashSet<Vector2Int>(occupiedTilesBuffer);
            
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
            
            //building.PositionInGrid = tile;
            //gridObjects.Add(tile, building);
            return obj;
        }

        public bool TileIsOccupied(Vector2Int tile) {
            return false; //gridObjects.ContainsKey(tile);
        }
    }
}