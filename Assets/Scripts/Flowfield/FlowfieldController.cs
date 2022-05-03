using System;
using System.Collections.Generic;
using EasyButtons;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Flowfield {
    public class FlowfieldController : MonoBehaviour {
        [FormerlySerializedAs("_size")] [SerializeField] private Vector2Int _gridSize;
        [SerializeField] private float _cellSize;
        [SerializeField] private FlowfieldCell[] _allCells;
        [SerializeField] private float _gizmosSphereSize = 0.2f;
        [SerializeField] private Terrain _terrain;
        [SerializeField] private float _angleThreshold = 35f;

        private float3 _origin => transform.position;

        private void Update() {
            //debug
            if (Input.GetKeyDown(KeyCode.F)) {
                var world = InputUtility.MouseToWorld(Camera.main, true);
                CreateIntegrationField(world);
            }
        }

        [Button]
        public void InitializeGrid() {
            _allCells = new FlowfieldCell[_gridSize.x * _gridSize.y];
            int i = 0;
            var pos = transform.position;
            for (int x = 0; x < _gridSize.x; x++) {
                for (int z = 0; z < _gridSize.y; z++) {
                    var flowFieldCell = new FlowfieldCell();
                    var posX = pos.x + x * _cellSize;
                    var posZ = pos.z + z * _cellSize;
                    var posY = _terrain.SampleHeight(new Vector3(posX, pos.y, posZ));
                    var position = new float3(posX, posY, posZ);
                    var center = new float3(posX + _cellSize / 2, posY, posZ + _cellSize / 2);
                    
                    flowFieldCell.WorldPosition = position;
                    flowFieldCell.Index = CalculateIndexFromInitialization(x, z);
                    flowFieldCell.WorldCenter = center;
                    flowFieldCell.Size = _cellSize;
                    flowFieldCell.GridPosition = new int2(x, z);
                    flowFieldCell.BestCost = float.MaxValue;
                    
                    flowFieldCell = FindBaseCost(center, posX, posY, posZ, flowFieldCell);
                    _allCells[i] = flowFieldCell;
                    i++;
                }
            }
        }
        
        private FlowfieldCell FindBaseCost(float3 center, float posX, float posY, float posZ, FlowfieldCell flowFieldCell) {
            // Test normals from center of cell and left bottom edge of cell against world up vector.
            // If angle is bigger than some threshold, it means that surface is too vertical and we can't move on it.
            var rayCenter = Physics.Raycast(new Vector3(center.x, center.y + 10, center.z), Vector3.down, out var hitCenter);
            var rayLeftBottomEdge = Physics.Raycast(new Vector3(posX, posY + 10, posZ), Vector3.down, out var hitEdge);
            if (rayCenter && rayLeftBottomEdge) {
                var normalCenter = hitCenter.normal;
                var normalLeftBottom = hitEdge.normal;
                var angleCenter = Vector3.Angle(Vector3.up, normalCenter);
                var angleLeftBottom = Vector3.Angle(Vector3.up, normalLeftBottom);
                var baseCost = 0f;
                if (angleCenter > _angleThreshold || angleLeftBottom > _angleThreshold) {
                    baseCost = float.MaxValue;
                } else {
                    baseCost = GrowExponential(_terrain.SampleHeight(center));
                }
                flowFieldCell.BaseCost = baseCost;
                flowFieldCell.NormalCenter = normalCenter;
                flowFieldCell.NormalEdge = normalLeftBottom;
                flowFieldCell.NormalXWorldUpAngle = angleCenter;
            }
            return flowFieldCell;
        }
        
        public void CreateIntegrationField(Vector3 end) {
            if (TileOutOfGrid(ToGrid(end), _gridSize.ToInt2())) {
                return;
            }
            var openList = new Queue<FlowfieldCell>();
            var endCell = _allCells[CalculateIndexFromWorld(end.x, end.z)];
            endCell.BaseCost = 0;
            endCell.BestCost = 0;
            openList.Enqueue(endCell);

            while (openList.Count > 0) {
                var currentCell = openList.Dequeue();
                var neighbours = GetNeighbourOffsets();

                for (var i = 0; i < neighbours.Length; i++) {
                    var offset = neighbours[i];
                    var neighbourPosition = new float3(currentCell.WorldPosition.x + offset.x, currentCell.WorldPosition.y, currentCell.WorldPosition.z + offset.y);
                    if (TileOutOfGrid(ToGrid(neighbourPosition), _gridSize.ToInt2())) {
                        continue;
                    }
                    var neighbourIndex = CalculateIndexFromWorld(neighbourPosition.x, neighbourPosition.z);
                    var neighbour = _allCells[neighbourIndex];
                    if (neighbour.BaseCost == float.MaxValue) {
                        continue;
                    }

                    var totalCost = neighbour.BaseCost + currentCell.BestCost;
                    if (totalCost < neighbour.BestCost) {
                        neighbour.BestCost = totalCost;
                        _allCells[neighbourIndex] = neighbour;
                        openList.Enqueue(neighbour);
                    }
                }

                neighbours.Dispose();
            }
        }

        private float GrowExponential(float value, float square = 3) {
            return 2 * Mathf.Pow(value, square);
        }

        private void OnDrawGizmos() {
            if (_allCells == null) return;
            foreach (var cell in _allCells) {
                Gizmos.color = Color.green;
                var rightBottom = new Vector3(cell.WorldPosition.x + _cellSize, cell.WorldPosition.y, cell.WorldPosition.z);
                var rightTop = new Vector3(cell.WorldPosition.x + _cellSize, cell.WorldPosition.y, cell.WorldPosition.z + _cellSize);
                var leftTop = new Vector3(cell.WorldPosition.x, cell.WorldPosition.y, cell.WorldPosition.z + _cellSize);
                Gizmos.DrawLine(cell.WorldPosition, rightBottom);
                Gizmos.DrawLine(rightBottom, rightTop);
                Gizmos.DrawLine(rightTop, leftTop);
                Gizmos.DrawLine(leftTop, cell.WorldPosition);

                if (cell.BaseCost == float.MaxValue) {
                    Gizmos.color = Color.red;
                } else {
                    Gizmos.color = Color.blue;
                }
                Gizmos.DrawLine(cell.WorldCenter, cell.WorldCenter + cell.NormalCenter * 5);
                Gizmos.DrawLine(cell.WorldPosition, cell.WorldPosition + cell.NormalEdge * 5);

                Gizmos.color = Color.white;

                var text = (cell.BaseCost == float.MaxValue || cell.BestCost == float.MaxValue) ? "MAX" : cell.BestCost.ToString("F");
                Handles.Label(cell.WorldCenter, text);
            }
        }
        
        private int CalculateIndexFromWorld(float worldX, float worldZ) {
            var gridPos = ToGrid(new float3(worldX, 0, worldZ));
            var max = Mathf.Max(_gridSize.x, _gridSize.y);
            var min = Mathf.Min(_gridSize.x, _gridSize.y);
            var index = (gridPos.x * _gridSize.x + gridPos.y) + ((max - min) * gridPos.x);
            // Debug.Log($"Calculate index. Max: {max}. Min: {min}. x: {x.ToString()}. y: {y.ToString()}. Index: {index}. Indexer: {indexer}");
            return Convert.ToInt32(index);
        }

        private int CalculateIndexFromInitialization(float x, float z) {
            var max = Mathf.Max(_gridSize.x, _gridSize.y);
            var min = Mathf.Min(_gridSize.x, _gridSize.y);
            var index = (x * _gridSize.x + z) + ((max - min) * x);
            return Convert.ToInt32(index);
        }

        private int2 ToGrid(float3 worldPos) {
            var localPosition = worldPos - _origin;
            int x = Mathf.FloorToInt(localPosition.x / _cellSize);
            int z = Mathf.FloorToInt(localPosition.z / _cellSize);
            return new int2(x, z);
        }

        private NativeArray<int2> GetNeighbourOffsets() {
            NativeArray<int2> offsets = new NativeArray<int2>(8, Allocator.Temp);
            offsets[0] = new int2(-1, 0);
            offsets[1] = new int2(1, 0);
            offsets[2] = new int2(0, 1);
            offsets[3] = new int2(0, -1);
            offsets[4] = new int2(-1, -1);
            offsets[5] = new int2(-1, 1);
            offsets[6] = new int2(1, -1);
            offsets[7] = new int2(1, 1);
            return offsets;
        }
        
        private bool TileOutOfGrid(float2 gridPos, int2 gridSize) {
            return gridPos.x < 0 || gridPos.y < 0 || gridPos.x > gridSize.x - 1 || gridPos.y > gridSize.y - 1;
        }
    }
}