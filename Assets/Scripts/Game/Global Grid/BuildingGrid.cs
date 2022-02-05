using System.Collections.Generic;
using Game.Buildings;
using UnityEngine;
using Utils;

namespace Game {
    public static class BuildingGrid {
        public static int Width => width;
        public static int Height => height;
        public static float CellSize => cellSize;
        public static bool Inited { get; private set; }
        
        private static int width;
        private static int height;
        private static float cellSize;
        private static Transform parent;
        private static Terrain terrain;
        private static List<Vector2Int> occupiedTilesBuffer = new();
        private static Dictionary<Vector2Int, GridTile> tiles = new();

        public static void Init(int gridWidth, int gridHeight, float gridCellSize, Transform gridParent, Terrain gridTerrain) {
            if (Inited) return;
            width = gridWidth;
            height = gridHeight;
            cellSize = gridCellSize;
            parent = gridParent;
            terrain = gridTerrain;
            Inited = true;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Vector2Int pos = new Vector2Int(x, y);
                    tiles.Add(pos, null);
                }
            }
        }

        public static Vector3 GridToWorld(Vector2Int cell) {
            Vector3 world = parent.TransformPoint(cell.ToVector3XZ() * cellSize);
            world.y = terrain.SampleHeight(world);
            return world;
        }

        public static Vector3 GridToWorldCentered(Vector2Int cell) {
            Vector3 world = parent.TransformPoint(cell.ToVector3XZ().CenterXZ() * cellSize);
            world.y = terrain.SampleHeight(world);
            return world;
        }
        
        public static Vector2Int WorldToGridFloored(Vector3 world) {
            return (parent.InverseTransformPoint(world) / cellSize).FloorToVector2IntXZ();
        }

        public static GameObject InstantiateOnGrid(Vector3 inWorldPos, GameObject prefab, Quaternion rotation, Transform parent) {
            if (!InstantiateInternal(prefab, rotation, parent, out var obj, out var building)) return null;
            SetSpawnPosition(inWorldPos, obj);
            building.Init();
            occupiedTilesBuffer.Clear();
            occupiedTilesBuffer.AddRange(building.positionsInGrid);
            foreach (var VARIABLE in occupiedTilesBuffer) {
                Debug.Log(VARIABLE);
            }
            if (!isPlaceable()) return null;
            foreach (Vector2Int occupiedTile in occupiedTilesBuffer) {
                tiles[occupiedTile] = new GridTile(building);
            }
            return obj;
        }
        
        private static bool isPlaceable() {
            foreach (Vector2Int occupiedTile in occupiedTilesBuffer) {
                if (TileOutOfGrid(occupiedTile)) {
                    Debug.LogWarning("Can't instantiate: index was out of range");
                    return false;
                }
                if (TileIsOccupied(occupiedTile)) {
                    Debug.Log("Can't Instantiate: tile is occupied " + occupiedTile);
                    return false;
                }
            }
            return true;
        }

        private static void SetSpawnPosition(Vector3 inWorldPos, GameObject obj) {
            Vector2Int spawnTile = WorldToGridFloored(inWorldPos);
            Vector3 outWorldPos = GridToWorldCentered(spawnTile);
            obj.transform.position = outWorldPos;
        }
        
        private static bool InstantiateInternal(GameObject prefab, Quaternion rotation, Transform parent, out GameObject obj, out Building building) {
            if (prefab == null) {
                Debug.LogWarning("Can't instantiate: missing prefab reference");
                obj = null;
                building = null;
                return false;
            }
            obj = Object.Instantiate(prefab, Vector3.zero, rotation, parent);
            if (!obj.TryGetComponent(out building)) {
                Object.Destroy(obj);
                Debug.LogWarning("Object was destroyed due to missing building component.");
                return false;
            }
            return true;
        }

        public static bool TileIsOccupied(Vector2Int tile) {
            if (TileOutOfGrid(tile)) {
                return true;
            }
            return tiles[tile] != null;
        }

        public static bool TileOutOfGrid(Vector2Int tile) {
            return tile.x < 0 || tile.x >= width || tile.y < 0 || tile.y >= height;
        }
    }
}