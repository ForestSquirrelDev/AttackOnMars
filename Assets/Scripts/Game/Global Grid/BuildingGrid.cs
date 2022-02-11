using System.Collections.Generic;
using Unity.Entities;
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
        private static Dictionary<Vector2Int, Entity> tiles = new Dictionary<Vector2Int, Entity>();

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
                    tiles.Add(pos, default);
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

        public static Vector2Int WorldToGridCeiled(Vector3 world) {
            return (parent.InverseTransformPoint(world) / cellSize).CeilToVector2IntXZ();
        }

        public static Vector3 WorldToGridCentered(Vector3 world) {
            Vector2Int grid = WorldToGridFloored(world);
            return GridToWorldCentered(grid);
        }
        
        public static bool TileIsOccupied(Vector2Int tile) {
            if (TileOutOfGrid(tile)) {
                return true;
            }
            return tiles[tile] != default;
        }

        public static bool TileOutOfGrid(Vector2Int tile) {
            return tile.x < 0 || tile.x >= width || tile.y < 0 || tile.y >= height;
        }
    }
}