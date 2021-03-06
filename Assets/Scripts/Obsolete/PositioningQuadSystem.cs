/*using System.Collections.Generic;
using System.Diagnostics;
using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public class PositioningQuadSystem : ComponentSystem {
        private Matrix4x4 _transformCenter;
        private Matrix4x4 _gridOrigin;
        private LocalToWorld _localToWorld;
        private PositioningGrid _positioningGrid;
        private List<int2> _localGridTiles = new List<int2>();
        private List<Vector3> _worldGridTiles = new List<Vector3>();
        private List<int2> _globalGridTiles = new List<int2>();
        private GridKeeperSystem _gridKeeperSystem;

        protected override void OnCreate() {
            _gridKeeperSystem = World.GetOrCreateSystem<GridKeeperSystem>();
        }

        protected override void OnUpdate() {
            return;
            Entities.WithAll<Tag_BuildingGhostPositioningQuad>().ForEach((DynamicBuffer<Int2BufferElement> buffer, ref LocalToWorld localToWorld) => {
                SetPositionsInGrid(localToWorld, buffer);
            });
            Entities.WithAll<Tag_BuildingPositioningQuad>().ForEach((DynamicBuffer<Int2BufferElement> buffer, ref LocalToWorld localToWorld,
                ref PositioningQuadComponent positioningQuad, ref Parent parent) => {
                //if (positioningQuad.inited) return;
                SetPositionsInGrid(localToWorld, buffer);
                //_gridKeeperSystem.buildingGrid.AddBuildingToGrid(_globalGridTiles, parent.Value);
                //positioningQuad.inited = true;
            });
        }

        public List<int2> GetPositionsInGrid() => _globalGridTiles;

        private void SetPositionsInGrid(LocalToWorld localToWorld, DynamicBuffer<Int2BufferElement> buffer) {
            this._localToWorld = localToWorld;
            Matrix4x4Extensions.AxesWiseMatrix(ref _transformCenter, localToWorld.Right, localToWorld.Forward, localToWorld.Up, localToWorld.Position);
            Matrix4x4Extensions.AxesWiseMatrix(ref _gridOrigin, _localToWorld.Right, _localToWorld.Forward, _localToWorld.Up,
                _transformCenter.MultiplyPoint3x4(new Vector3(-0.5f, 0f, -0.5f)));
            GetOccupiedGlobalGridTiles();
            for (int i = 0; i < _globalGridTiles.Count; i++) {
                if (buffer.Length >= _globalGridTiles.Count) {
                    buffer[i] = new Int2BufferElement { value = _globalGridTiles[i] };
                } else {
                    buffer.Add(new Int2BufferElement { value = _globalGridTiles[i] });
                }
            }
        }

        private void GetOccupiedGlobalGridTiles() {
            int2 size = CalculateGridSize();
            _positioningGrid = new PositioningGrid();
            _positioningGrid.FillGrid(size.x, size.y);
            _localGridTiles.AddRange(_positioningGrid.positions);
            _positioningGrid.Dispose();
            FillWorldGrid();
            FillGlobalGrid();
        }

        private void FillWorldGrid() {
            _worldGridTiles.Clear();
            Matrix4x4Extensions.ToUnitScale(ref _gridOrigin);
            foreach (var tile in _localGridTiles) {
                Vector3 world = _gridOrigin.MultiplyPoint3x4(new Vector3(tile.x, 0, tile.y) * _gridKeeperSystem.BuildingGrid.CellSize);
                _worldGridTiles.Add(world);
            }
        }
        
        private void FillGlobalGrid() {
            _globalGridTiles.Clear();
            foreach (var tile in _worldGridTiles) {
                _globalGridTiles.Add(_gridKeeperSystem.BuildingGrid.WorldToGridFloored(tile).ToInt2());
            }
        }
        
        private int2 CalculateGridSize() {
            float3 leftBottomCorner = new float3(-0.5f, 0f, -0.5f);
            float3 leftTopCorner = new float3(-0.5f, 0, 0.5f);
            float3 rightBottomCorner = new float3(0.5f, 0f, -0.5f);

            leftBottomCorner = _transformCenter.MultiplyPoint3x4(leftBottomCorner);
            leftTopCorner = _transformCenter.MultiplyPoint3x4(leftTopCorner);
            rightBottomCorner = _transformCenter.MultiplyPoint3x4(rightBottomCorner);

            int2 leftBottomToGlobalGrid = _gridKeeperSystem.BuildingGrid.WorldToGridCeiled(leftBottomCorner).ToInt2();
            int2 leftTopToGlobalGrid = _gridKeeperSystem.BuildingGrid.WorldToGridCeiled(leftTopCorner).ToInt2();
            int2 rightBottomToGlobalGrid = _gridKeeperSystem.BuildingGrid.WorldToGridCeiled(rightBottomCorner).ToInt2();
            
            int width = math.clamp(math.abs(rightBottomToGlobalGrid.x - leftBottomToGlobalGrid.x) + 1, 1, int.MaxValue);
            int height = math.clamp(math.abs(leftTopToGlobalGrid.y - leftBottomToGlobalGrid.y) + 1, 1, int.MaxValue);

            return new int2(width, height);
        }

        [Conditional("UNITY_EDITOR")]
        public void OnDrawGizmos() {
            Gizmos.color = Color.green;
            foreach (var v in _worldGridTiles) {
                Gizmos.DrawSphere(_gridKeeperSystem.BuildingGrid.WorldToGridCentered(v), 0.5f);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_gridOrigin.GetColumn(3), _gridOrigin.GetColumn(3) + _gridOrigin.GetColumn(0));
            Gizmos.DrawLine(_transformCenter.GetColumn(3), _transformCenter.GetColumn(3) + _transformCenter.GetColumn(0));
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_gridOrigin.GetColumn(3), _gridOrigin.GetColumn(3) + _gridOrigin.GetColumn(1));
            Gizmos.DrawLine(_transformCenter.GetColumn(3), _transformCenter.GetColumn(3) + _transformCenter.GetColumn(1));
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_gridOrigin.GetColumn(3), _gridOrigin.GetColumn(3) + _gridOrigin.GetColumn(2));
            Gizmos.DrawLine(_transformCenter.GetColumn(3), _transformCenter.GetColumn(3) + _transformCenter.GetColumn(2));
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_transformCenter.MultiplyPoint3x4(new Vector3(-.5f, 0, .5f)), .5f);
        }
    }
}*/