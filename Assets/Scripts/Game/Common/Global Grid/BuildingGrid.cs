using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;

namespace Game {
    public struct BuildingGrid {
        public float CellSize => _cellSize;

        private int _width;
        private int _height;
        private float _cellSize;
        private float4x4 _localToWorld4x4;
        private float4x4 _worldToLocal4x4;
        private NativeHashMap<float3, float> _sampledHeights;
        private NativeHashMap<int2, Entity> _tiles;
        
        public void Init(int gridWidth, int gridHeight, float gridCellSize, Matrix4x4 localToWorld, Matrix4x4 worldToLocal, int sampledHeightsCapacity) {
            _width = gridWidth;
            _height = gridHeight;
            _cellSize = gridCellSize;
            _tiles = new NativeHashMap<int2, Entity>(_width * _height, Allocator.Persistent);
            _sampledHeights = new NativeHashMap<float3, float>(sampledHeightsCapacity, Allocator.Persistent);
            for (int x = 0; x < _width; x++) {
                for (int y = 0; y < _height; y++) {
                    int2 pos = new int2(x, y);
                    _tiles.Add(pos, Entity.Null);
                }
            }
            _localToWorld4x4 = localToWorld;
            _worldToLocal4x4 = worldToLocal;
        }

        public void SetSampledHeight(float3 world, float height) {
            _sampledHeights[world] = height;
        }

        [return: ReadOnly]
        public Vector3 GridToWorld(Vector2Int cell, bool sampleHeight = true) {
            Vector3 world = _localToWorld4x4.MultiplyPoint((cell.ToVector3XZ() * _cellSize).ToFloat4(1)).ToVector4();
            if (sampleHeight)
                world.y = _sampledHeights[world];
            return world;
        }

        [return: ReadOnly]
        public Vector3 GridToWorldCentered(Vector2Int cell, bool sampleHeight = true) {
            Vector3 world = _localToWorld4x4.MultiplyPoint((cell.ToVector3XZ().CenterXZ() * _cellSize).ToFloat4(1)).ToVector4();
            if (sampleHeight)
                world.y = _sampledHeights[world];
            return world;
        }
        
        [return: ReadOnly]
        public Vector2Int WorldToGridFloored(Vector3 world) {
            return (_worldToLocal4x4.MultiplyPoint(world.ToFloat4(1)) / _cellSize).FloorToVector2IntXZ();
        }

        [return: ReadOnly]
        public Vector2Int WorldToGridCeiled(Vector3 world) {
            return (_worldToLocal4x4.MultiplyPoint(world.ToFloat4(1)) / _cellSize).CeilToVector2IntXZ();
        }

        [return: ReadOnly]
        public Vector3 WorldToGridCentered(Vector3 world, bool sampleHeight = true) {
            Vector2Int grid = WorldToGridFloored(world);
            return GridToWorldCentered(grid, sampleHeight);
        }
        
        [return: ReadOnly]
        public bool TileIsOccupied(Vector2Int tile) {
            return TileOutOfGrid(tile) || _tiles[tile.ToInt2()] != Entity.Null;
        }

        [return: ReadOnly]
        public bool TileOutOfGrid(Vector2Int tile) {
            return tile.x < 0 || tile.x >= _width || tile.y < 0 || tile.y >= _height;
        }
        
        /// <param name="rect">Expects rect with worldspace xMin, xMax, yMin, yMax</param>
        [return: ReadOnly]
        public bool IntersectsWithOccupiedTiles(Rect rect) {
            Vector3 xzMin = new Vector3(rect.xMin, 0f, rect.yMin);
            Vector3 xzMax = new Vector3(rect.xMax, 0f, rect.yMax);
            
            Vector2Int xzMinGrid = WorldToGridFloored(xzMin);
            Vector2Int xzMaxGrid = WorldToGridFloored(xzMax);
            rect.xMin = xzMinGrid.x;
            rect.yMin = xzMinGrid.y;
            rect.xMax = xzMaxGrid.x;
            rect.yMax = xzMaxGrid.y;

            for (int x = (int)rect.xMin; x <= rect.xMax; x++) {
                for (int z = (int)rect.yMin; z <= rect.yMax; z++) {
                    if (TileIsOccupied(new Vector2Int(x, z))) return true;
                }
            }
            
            return false;
        }
        
        public void AddBuildingToGrid(NativeArray<int2> tiles, Entity entity) {
            for (var i = 0; i < tiles.Length; i++) {
                var tile = tiles[i];
                if (_tiles.ContainsKey(tile)) {
                    _tiles[tile] = entity;
                }
            }
        }

        public bool InstantiateOnGrid(Vector3 inWorldPos, Entity entityIn, EntityManager manager, out Entity entityOut) {
            if (entityIn == Entity.Null) {
                Debug.LogError("Can't instantiate as the entity is null");
                entityOut = Entity.Null;
                return false;
            }
            Vector2Int spawnTile = WorldToGridFloored(inWorldPos);
            entityOut = manager.Instantiate(entityIn);
            manager.SetComponentData(entityOut, new Translation {Value = GridToWorldCentered(spawnTile)});
            return true;
        }

        public void Dispose() {
            _tiles.Dispose();
            _sampledHeights.Dispose();
        }

        [Conditional("UNITY_EDITOR")]
        public void DebugTiles() {
            foreach (var tile in _tiles) {
                Debug.Log($"key: {tile.Key}, value: {tile.Value}");
            }
        }
        
        [Conditional("UNITY_EDITOR")]
        public void EditorInit(int gridWidth, int gridHeight, float gridCellSize, Matrix4x4 localToWorld, Matrix4x4 worldToLocal) {
            _width = gridWidth;
            _height = gridHeight;
            _cellSize = gridCellSize;
            _localToWorld4x4 = localToWorld;
            _worldToLocal4x4 = worldToLocal;
        }
    }
}