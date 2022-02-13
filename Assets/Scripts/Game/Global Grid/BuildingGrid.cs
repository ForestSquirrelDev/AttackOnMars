using System.Collections.Generic;
using System.Diagnostics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;

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
        private static Matrix4x4 localToWorld;
        private static Matrix4x4 worldToLocal;
        private static Dictionary<Vector2Int, Entity> tiles = new Dictionary<Vector2Int, Entity>();

        public static void Init(int gridWidth, int gridHeight, float gridCellSize, Transform gridParent, Terrain gridTerrain) {
            tiles.Clear();
            width = gridWidth;
            height = gridHeight;
            cellSize = gridCellSize;
            parent = gridParent;
            terrain = gridTerrain;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Vector2Int pos = new Vector2Int(x, y);
                    tiles.Add(pos, Entity.Null);
                }
            }
            localToWorld = parent.localToWorldMatrix;
            worldToLocal = parent.worldToLocalMatrix;
        }

        public static Vector3 GridToWorld(Vector2Int cell) {
            Vector3 world = localToWorld.MultiplyPoint3x4(cell.ToVector3XZ() * cellSize);
            world.y = terrain.SampleHeight(world);
            return world;
        }

        public static Vector3 GridToWorldCentered(Vector2Int cell) {
            Vector3 world = localToWorld.MultiplyPoint3x4(cell.ToVector3XZ().CenterXZ() * cellSize);
            world.y = terrain.SampleHeight(world);
            return world;
        }
        
        public static Vector2Int WorldToGridFloored(Vector3 world) {
            return (worldToLocal.MultiplyPoint3x4(world) / cellSize).FloorToVector2IntXZ();
        }

        public static Vector2Int WorldToGridCeiled(Vector3 world) {
            return (worldToLocal.MultiplyPoint3x4(world) / cellSize).CeilToVector2IntXZ();
        }

        public static Vector3 WorldToGridCentered(Vector3 world) {
            Vector2Int grid = WorldToGridFloored(world);
            return GridToWorldCentered(grid);
        }
        
        public static bool TileIsOccupied(Vector2Int tile) {
            return TileOutOfGrid(tile) || tiles[tile] != Entity.Null;
        }

        public static bool TileOutOfGrid(Vector2Int tile) {
            return tile.x < 0 || tile.x >= width || tile.y < 0 || tile.y >= height;
        }

        public static void AddBuildingToGrid(IEnumerable<int2> tiles, Entity entity) {
            foreach (var tile in tiles) {
                Vector2Int v = tile.ToVector2Int();
                if (BuildingGrid.tiles.ContainsKey(v)) {
                    BuildingGrid.tiles[v] = entity;
                }
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void DebugTiles() {
            foreach (var tile in tiles) {
                Debug.Log($"key: {tile.Key}, value: {tile.Value}");
            }
        }
    }
}