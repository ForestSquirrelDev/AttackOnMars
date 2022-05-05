using System;
using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Utils.Maths;

namespace Flowfield {
    public class FlowfieldController : MonoBehaviour {
        [FormerlySerializedAs("_size")] [SerializeField] private Vector2Int _gridSize;
        [SerializeField] private float _cellSize;
        [SerializeField] private FlowfieldCell[] _allCells;
        [SerializeField] private float _gizmosSphereSize = 0.2f;
        [SerializeField] private Terrain _terrain;
        [SerializeField] private float _angleThreshold = 35f;
        
        [SerializeField] private bool _debugNormals = true;
        [SerializeField] private bool _debugCosts = true;
        [SerializeField] private bool _drawArrows = true;
        [SerializeField] private bool _debugPositions;

        private float3 _origin => transform.position;

        private void Update() {
            //debug
            if (Input.GetKeyDown(KeyCode.F)) {
                var world = InputUtility.MouseToWorld(Camera.main, true);
                StartCoroutine(CreateIntegrationField(world));
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
                    flowFieldCell.Index = CalculateIndexFromGrid(x, z);
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
                    baseCost = _terrain.SampleHeight(center);
                }
                flowFieldCell.BaseCost = baseCost;
                flowFieldCell.NormalCenter = normalCenter;
                flowFieldCell.NormalEdge = normalLeftBottom;
                flowFieldCell.NormalXWorldUpAngle = angleCenter;
            }
            return flowFieldCell;
        }
        
        private IEnumerator CreateIntegrationField(Vector3 end) {
            if (TileOutOfGrid(ToGrid(end), _gridSize.ToInt2())) {
                yield break;
            }
            var openList = new Queue<FlowfieldCell>();
            var closedList = new NativeList<FlowfieldCell>(Allocator.Persistent);
            var endCell = _allCells[CalculateIndexFromWorld(end.x, end.z)];
            Debug.Log($"End cell: {endCell.GridPosition}");
            endCell.BaseCost = 0;
            endCell.BestCost = 0;
            _allCells[endCell.Index] = endCell;
            openList.Enqueue(endCell);

            while (openList.Count > 0) {
                var currentCell = openList.Dequeue();
                var neighbours = FindNeighbours(currentCell, _allCells);
                var validNeighbours = new NativeList<FlowfieldCell>(Allocator.Persistent);
                for (var i = 0; i < neighbours.Length; i++) {
                    var neighbour = neighbours[i];
                    if (TileOutOfGrid(neighbour.GridPosition, _gridSize.ToInt2()) 
                        || neighbour.BaseCost == float.MaxValue || closedList.Contains(neighbour)) {
                        continue;
                    }
                    var totalCost = neighbour.BaseCost + currentCell.BestCost;
                    if (totalCost < neighbour.BestCost) {
                        neighbour.BestCost = totalCost;
                        _allCells[neighbour.Index] = neighbour;
                        if (!openList.Contains(neighbour))
                            openList.Enqueue(neighbour);
                        closedList.Add(neighbour);
                        validNeighbours.Add(neighbour);
                    }
                    yield return null;
                }
                validNeighbours.Dispose();
                neighbours.Dispose();
            }

            closedList.Dispose();
            
            CreateFlowField(_allCells);
        }

        private void CreateFlowField(FlowfieldCell[] allCells) {
            for (var i = 0; i < allCells.Length; i++) {
                var cell = allCells[i];
                var neighbours = FindNeighbours(cell, allCells);
                var bestDirection = FindBestDirection(cell, neighbours);
                cell.BestDirection = bestDirection;
                allCells[cell.Index] = cell;
                neighbours.Dispose();
            }
        }

        private int2 FindBestDirection(FlowfieldCell currentCell, NativeArray<FlowfieldCell> validNeighbours) {
            var bestCell = FindLowestCostCellSlow(validNeighbours);
            Debug.Log($"Current cell pos: {currentCell.GridPosition}. BestCell pos: {bestCell.GridPosition}. Vec: {currentCell.GridPosition - bestCell.GridPosition}");
            return bestCell.GridPosition - currentCell.GridPosition;
        }

        private FlowfieldCell FindLowestCostCellSlow(NativeArray<FlowfieldCell> neighbours) {
            var bestCell = new FlowfieldCell {
                BestCost = float.MaxValue,
                Index = -1
            };

            for (var i = 0; i < neighbours.Length; i++) {
                var currentCell = neighbours[i];
                if (currentCell.BestCost < bestCell.BestCost) {
                   bestCell = currentCell;
                }
            }
            if (bestCell.Index == -1) {
                Debug.LogError("Couldn't find best cell");
            }
            return bestCell;
        }

        private NativeList<FlowfieldCell> FindNeighbours(FlowfieldCell currentCell, FlowfieldCell[] allCells) {
            var neighbourOffsets = GetNeighbourOffsets();
            var neighbours = new NativeList<FlowfieldCell>(Allocator.Persistent);

            for (var i = 0; i < neighbourOffsets.Length; i++) {
                var neighbourOffset = neighbourOffsets[i];
                var neighbourGridPosition = new int2(currentCell.GridPosition.x + neighbourOffset.x, currentCell.GridPosition.y + neighbourOffset.y);
                if (TileOutOfGrid(neighbourGridPosition, _gridSize.ToInt2()))
                    continue;
                var neighbourIndex = CalculateIndexFromGrid(neighbourGridPosition.x, neighbourGridPosition.y);
                var neighbourCell = allCells[neighbourIndex];
                neighbours.Add(neighbourCell);
            }

            neighbourOffsets.Dispose();
            return neighbours;
        }

        private void OnDrawGizmos() {
            if (_allCells == null) return;
            foreach (var cell in _allCells) {
                Gizmos.color = cell.BestCost == float.MaxValue ? Color.red : Color.green;
                var rightBottom = new Vector3(cell.WorldPosition.x + _cellSize, cell.WorldPosition.y, cell.WorldPosition.z);
                var rightTop = new Vector3(cell.WorldPosition.x + _cellSize, cell.WorldPosition.y, cell.WorldPosition.z + _cellSize);
                var leftTop = new Vector3(cell.WorldPosition.x, cell.WorldPosition.y, cell.WorldPosition.z + _cellSize);
                Gizmos.DrawLine(cell.WorldPosition, rightBottom);
                Gizmos.DrawLine(rightBottom, rightTop);
                Gizmos.DrawLine(rightTop, leftTop);
                Gizmos.DrawLine(leftTop, cell.WorldPosition);

                if (_debugNormals) {
                    if (cell.BaseCost == float.MaxValue) {
                        Gizmos.color = Color.red;
                    } else {
                        Gizmos.color = Color.blue;
                    }
                    Gizmos.DrawLine(cell.WorldCenter, cell.WorldCenter + cell.NormalCenter * 5);
                    Gizmos.DrawLine(cell.WorldPosition, cell.WorldPosition + cell.NormalEdge * 5);
                }

                if (_debugCosts) {
                    Handles.color = Color.magenta;

                    var text = (cell.BaseCost == float.MaxValue || cell.BestCost == float.MaxValue) ? "MAX" : (cell.BestCost).ToString("0");
                    Handles.Label(cell.WorldCenter, text);
                }

                if (_debugPositions) {
                    var text = cell.GridPosition.ToString();
                    Handles.Label(cell.WorldPosition, text);
                }

                if (_drawArrows && !(cell.BestDirection.x == 0 && cell.BestDirection.y == 0)) {
                    Gizmos.color = Color.cyan;
                    var arrowTip = new Vector3(cell.WorldCenter.x + cell.BestDirection.x, cell.WorldCenter.y, cell.WorldCenter.z + cell.BestDirection.y);
                    Gizmos.DrawLine(cell.WorldCenter, arrowTip);
                    var middle = ((Vector3)cell.WorldCenter + arrowTip) * 0.5f;
                    var AB = (Vector3)cell.WorldCenter - arrowTip;
                    var middleRight = Vector3.Cross(Vector3.down, AB).normalized;
                    var middleLeft = Vector3.Cross(AB, Vector3.down).normalized;
                    Gizmos.DrawLine(arrowTip, (Vector3)middle + middleRight);
                    Gizmos.DrawLine(arrowTip, (Vector3)middle + middleLeft);
                }
                Gizmos.DrawSphere(cell.WorldPosition, 0.1f);
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

        private int CalculateIndexFromGrid(float x, float z) {
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