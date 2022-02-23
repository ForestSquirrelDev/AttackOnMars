using System.Collections.Generic;
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
        
        public void Init(int gridWidth, int gridHeight, float gridCellSize, Matrix4x4 localToWorld, Matrix4x4 worldToLocal, int sampledHeightsCapacity, bool initUnsafeCollections = false) {
            _width = gridWidth;
            _height = gridHeight;
            _cellSize = gridCellSize;
            if (initUnsafeCollections) {
                _tiles = new NativeHashMap<int2, Entity>(_width * _height, Allocator.Persistent);
                _sampledHeights = new NativeHashMap<float3, float>(sampledHeightsCapacity, Allocator.Persistent);
                for (int x = 0; x < _width; x++) {
                    for (int y = 0; y < _height; y++) {
                        int2 pos = new int2(x, y);
                        _tiles.Add(pos, Entity.Null);
                    }
                }
            }
            for (int i = 0; i <= 3; i++) {
                _localToWorld4x4[i] = localToWorld.GetColumn(i).ToFloat4();
                _worldToLocal4x4[i] = worldToLocal.GetColumn(i).ToFloat4();
            }
        }

        public void SetSampledHeight(float3 world, float height) {
            _sampledHeights.Add(world, height);
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

        [return: ReadOnly]
        public void AddBuildingToGrid(IEnumerable<int2> tiles, Entity entity) {
            foreach (var tile in tiles) {
                if (this._tiles.ContainsKey(tile)) {
                    this._tiles[tile] = entity;
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
            entityOut = InstantiateEcs(GridToWorldCentered(spawnTile), entityIn, manager);
            return true;
        }
        
        private Entity InstantiateEcs(Vector3 inWorldPos, Entity entity, EntityManager manager) {
            Entity building = manager.Instantiate(entity);
            manager.SetComponentData(building, new Translation {Value = inWorldPos});
            return building;
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
    }
}