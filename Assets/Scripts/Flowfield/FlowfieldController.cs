using System.Collections.Generic;
using System.Diagnostics;
using Den.Tools;
using EasyButtons;
using Game.Ecs.Utils;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utils;
using static Flowfield.FlowfieldUtility;
using Debug = UnityEngine.Debug;

namespace Flowfield {
    public class FlowfieldController : MonoBehaviour {
        [SerializeField] private int2 _parentGridSizeOut;
        [SerializeField] private int2 _gridSize;
        [SerializeField] private float _parentCellSize = 50f;
        [SerializeField] private float _tooBigHeightThreshold = 23f;
        [SerializeField] private float _smallCellSize = 6f;
        [SerializeField] private FlowFieldCell[] _allCells;
        [SerializeField] private Terrain _terrain;
        [SerializeField] private float _angleThreshold = 35f;
        
        [SerializeField] private bool _debugNormals = true;
        [SerializeField] private bool _debugCosts = true;
        [SerializeField] private bool _drawArrows = true;
        [SerializeField] private bool _debugPositions;
        [SerializeField] private bool _debugSmallGrids = true;
        [SerializeField] private bool _debugParentGrid = true;
        [SerializeField] private Transform _testEnemy;

        // closer to actual implementation in monobehaviour
        [SerializeField] private List<FlowFieldCell> _mergedCells = new List<FlowFieldCell>();
        
        private float3 _parentGridOrigin => _terrain.transform.position;

        private Stopwatch _sw = new Stopwatch();
        private FlowFieldCell _currentCell;
        
        private FlowFieldCell _targetCell;
        
        private void Update() {
            if (Input.GetKeyDown(KeyCode.F)) {
                var world = InputUtility.MouseToWorld(Camera.main, true);
                GenerateMergedGrid(_parentCellSize);
                CreateIntegrationField(world, _parentGridOrigin, _parentCellSize, _parentGridSizeOut, _mergedCells);
                CreateFlowField(_mergedCells, _parentGridSizeOut);
                var targetIndex = CalculateIndexFromWorld(world, _parentGridOrigin, _parentGridSizeOut, _parentCellSize);
                Debug.Log($"Target index: {targetIndex}");
                _targetCell = _mergedCells[targetIndex];
            }
            FindPath(_targetCell, _mergedCells, _testEnemy, _parentGridOrigin, _parentCellSize, _parentGridSizeOut);
        }

        private void FindPath(FlowFieldCell targetCell, IList<FlowFieldCell> allCells, Transform enemy, float3 gridOrigin, float cellSize, int2 gridSize) {
            if (targetCell == default) return;
            var enemyGridPosition = ToGrid(enemy.position, gridOrigin, cellSize);
            var arrived = enemyGridPosition.x == targetCell.GridPosition.x && enemyGridPosition.y == targetCell.GridPosition.y;
            Debug.Log($"Enemy grid position: {enemyGridPosition}. Target grid position: {_targetCell}. arrived: {arrived}");
            if (!arrived) {
                var currentCellIndex = CalculateIndexFromGrid(enemyGridPosition, gridSize);
                var currentCell = allCells[currentCellIndex];
                var bestDirection = math.normalize(currentCell.BestDirection);
                var worldDirection = new Vector3(bestDirection.x, 0, bestDirection.y);
                enemy.position += worldDirection * 60f * Time.deltaTime;
                Debug.Log($"Enemy grid pos: {currentCellIndex}. Current cell: {currentCell.GridPosition}. Best direction: {bestDirection}. World direction: {worldDirection}. Target cell: {_targetCell.ToString()}");
            }
        }

        [Button]
        private void GenerateMergedGrid(float cellSize) {
            var terrain = _terrain;
            var terrainRect = terrain.GetWorldRect();
            var origin = terrain.transform.position;
            var w = Mathf.FloorToInt(terrainRect.width / cellSize);
            var h = Mathf.FloorToInt(terrainRect.height / cellSize);
            var gridSize = new int2(w, h);
            _mergedCells = new List<FlowFieldCell>(gridSize.x * gridSize.y);

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++) {
                    var cell = new FlowFieldCell();
                    cell.GridPosition = new int2(x, y);
                    cell.WorldPosition = ToWorld(cell.GridPosition, origin, cellSize);
                    cell.WorldPosition.y = _terrain.SampleHeight(cell.WorldPosition);
                    cell.WorldCenter = FindCellCenter(cell.WorldPosition, cellSize);
                    cell.WorldCenter.y = _terrain.SampleHeight(cell.WorldCenter);
                    cell.GridPosition = new int2(x, y);
                    cell.Index = CalculateIndexFromGrid(cell.GridPosition, gridSize);
                    var cellRect = new FlowFieldRect {
                        X = cell.WorldPosition.x,
                        Y = cell.WorldPosition.z,
                        Height = cellSize,
                        Width = cellSize
                    };
                    cell.Size = cellSize;
                    cell.WorldRect = cellRect;
                    cell.BaseCost = FindBaseCost(cell.WorldCenter, cell.WorldPosition);
                    cell.BestCost = float.MaxValue;

                    _mergedCells.Add(cell);
                }
            _parentGridSizeOut = gridSize;
        }

        private void CreateIntegrationField(Vector3 targetWorldPosition, float3 gridOrigin, float cellSize, int2 gridSize, IList<FlowFieldCell> allCells) {
            _sw.Start();
            var endGridPos = ToGrid(targetWorldPosition, gridOrigin, cellSize);
            if (TileOutOfGrid(endGridPos, gridSize)) {
                return;
            }
            var openList = new NativeQueue<FlowFieldCell>(Allocator.Temp);
            var closedList = new NativeList<FlowFieldCell>(Allocator.Temp);
            var index = CalculateIndexFromWorld(targetWorldPosition, gridOrigin, gridSize, cellSize);
            var endCell = allCells[index];
            Debug.Log($"End cell: {endCell}. Index: {endCell.Index}");
            endCell.BaseCost = 0;
            endCell.BestCost = 0;
            allCells[endCell.Index] = endCell;
            openList.Enqueue(endCell);

            while (openList.Count > 0) {
                var currentCell = openList.Dequeue();
                var neighbours = FindNeighbours(currentCell, allCells, gridSize);
                Debug.Log($"Lesgo. Current cell: {currentCell}");
                for (var i = 0; i < neighbours.Length; i++) {
                    var neighbour = neighbours[i];
                    if (TileOutOfGrid(neighbour.GridPosition, gridSize) 
                        || neighbour.BaseCost == float.MaxValue || closedList.Contains(neighbour)) {
                        Debug.Log($"Tile {neighbour} out of grid. Continue");
                        continue;
                    }
                    var totalCost = neighbour.BaseCost + currentCell.BestCost;
                    Debug.Log($"Total cost: {totalCost}. Neighbour best cost: {neighbour.BestCost}. Better: {totalCost < neighbour.BestCost}" +
                              $" Neighbours count: {neighbours.Length}. Openlist count: {openList.Count}.");
                    if (totalCost < neighbour.BestCost) {
                        neighbour.BestCost = totalCost;
                        allCells[neighbour.Index] = neighbour;
                        openList.Enqueue(neighbour);
                        closedList.Add(neighbour);
                    }
                }
                neighbours.Dispose();
            }

            openList.Dispose();
            closedList.Dispose();
            _targetCell = endCell;
            _sw.Stop();
            Debug.Log($"Elapsed miliseconds: {_sw.ElapsedMilliseconds}");
            _sw.Reset();
        }
        
        private float FindBaseCost(float3 worldCenter, float3 worldPosition) {
            // Test normals from center of cell and left bottom edge of cell against world up vector.
            // If angle is bigger than some threshold, it means that surface is too vertical and we can't move on it.
            // Otherwise just set base cost depending on height.
            var rayCenter = Physics.Raycast(new Vector3(worldCenter.x, worldCenter.y + 10, worldCenter.z), Vector3.down, out var hitCenter);
            var rayLeftBottomEdge = Physics.Raycast(new Vector3(worldPosition.x, worldPosition.y + 10, worldPosition.z), Vector3.down, out var hitEdge);
            var baseCost = 0f;
            if (rayCenter && rayLeftBottomEdge) {
                var normalCenter = hitCenter.normal;
                var normalLeftBottom = hitEdge.normal;
                var angleCenter = Vector3.Angle(Vector3.up, normalCenter);
                var angleLeftBottom = Vector3.Angle(Vector3.up, normalLeftBottom);
                if (angleCenter > _angleThreshold || angleLeftBottom > _angleThreshold) {
                    baseCost = float.MaxValue;
                } else {
                    baseCost = 1f;
                    if (worldCenter.y > _tooBigHeightThreshold) {
                        baseCost += hitCenter.point.y;
                    }
                }
            }
            return baseCost;
        }

        private void CreateFlowField(IList<FlowFieldCell> allCells, int2 gridSize) {
            for (var i = 0; i < allCells.Count; i++) {
                var cell = allCells[i];
                var neighbours = FindNeighbours(cell, allCells, gridSize);
                var bestDirection = FindBestDirection(cell, neighbours);
                cell.BestDirection = bestDirection;
                allCells[cell.Index] = cell;
                neighbours.Dispose();
            }
        }

        private int2 FindBestDirection(FlowFieldCell currentCell, NativeArray<FlowFieldCell> validNeighbours) {
            var bestCell = FindLowestCostCellSlow(validNeighbours);
            return bestCell.GridPosition - currentCell.GridPosition;
        }

        private FlowFieldCell FindLowestCostCellSlow(NativeArray<FlowFieldCell> validNeighbours) {
            var bestCell = new FlowFieldCell {
                BestCost = float.MaxValue,
                Index = -1
            };
            Debug.Log($"Valid neighbours count: {validNeighbours.Length}");
            for (var i = 0; i < validNeighbours.Length; i++) {
                var currentCell = validNeighbours[i];
                
                if (currentCell.BestCost < bestCell.BestCost) {
                    bestCell = currentCell;
                }
            }
            if (bestCell.Index == -1) {
                Debug.LogError("Couldn't find best cell");
            }
            return bestCell;
        }

        private NativeList<FlowFieldCell> FindNeighbours(FlowFieldCell currentCell, IList<FlowFieldCell> allCells, int2 gridSize) {
            var neighbourOffsets = GetNeighbourOffsets();
            var neighbours = new NativeList<FlowFieldCell>(Allocator.Temp);

            for (var i = 0; i < neighbourOffsets.Length; i++) {
                var neighbourOffset = neighbourOffsets[i];
                var neighbourGridPosition = new int2(currentCell.GridPosition.x + neighbourOffset.x, currentCell.GridPosition.y + neighbourOffset.y);
                if (TileOutOfGrid(neighbourGridPosition, gridSize))
                    continue;
                var neighbourIndex = CalculateIndexFromGrid(neighbourGridPosition.x, neighbourGridPosition.y, gridSize);
                var neighbourCell = allCells[neighbourIndex];
                neighbours.Add(neighbourCell);
            }

            neighbourOffsets.Dispose();
            return neighbours;
        }
        #region gizmo

        private void OnDrawGizmos() {
            if (_allCells == null) return;
            if (_debugSmallGrids) {
                foreach (var cell in _allCells) {
                    Gizmos.color = cell.BestCost == float.MaxValue ? Color.red : Color.green;
                    DrawSingleCell(cell, _smallCellSize);

                    if (_debugNormals) {
                        if (cell.BaseCost == float.MaxValue) {
                            Gizmos.color = Color.red;
                        } else {
                            Gizmos.color = Color.blue;
                        }
                        // Gizmos.DrawLine(cell.WorldCenter, cell.WorldCenter + cell.NormalCenter * 5);
                        // Gizmos.DrawLine(cell.WorldPosition, cell.WorldPosition + cell.NormalEdge * 5);
                    }

                    if (_debugCosts) {
                        DrawCosts(cell);
                    }

                    if (_debugPositions) {
                        var text = cell.GridPosition.ToString();
                        Handles.Label(cell.WorldPosition, text);
                    }

                    if (_drawArrows && !(cell.BestDirection.x == 0 && cell.BestDirection.y == 0) && cell.BestCost != float.MaxValue) {
                        DrawSingleArrow(cell);
                    }
                }
            }
            
            if (_debugParentGrid) {
                foreach (var cell in _mergedCells) {
                    Gizmos.color = cell.BaseCost.Approximately(float.MaxValue)? Color.red : Color.green;
                    DrawSingleCell(cell, _parentCellSize, false);

                    if (_debugCosts) {
                        DrawCosts(cell);
                    }
                    if (_drawArrows && !(cell.BestDirection.x == 0 && cell.BestDirection.y == 0) && cell.BestCost != float.MaxValue) {
                        DrawSingleArrow(cell, 15f, 2.5f);
                    }
                    if (_debugPositions) {
                        var text = cell.GridPosition.ToString();
                        Handles.Label(cell.WorldPosition, text);
                    }
                }       
            }
        }
        
        private void DrawSingleArrow(FlowFieldCell cell, float length = 1f, float thickness = 1f) {
            Handles.color = Color.cyan;
            var arrowTip = new Vector3(cell.WorldCenter.x + cell.BestDirection.x * length, cell.WorldCenter.y, cell.WorldCenter.z + cell.BestDirection.y * length);
            Handles.DrawLine(cell.WorldCenter, arrowTip, thickness);
            var middle = ((Vector3)cell.WorldCenter + arrowTip) * 0.5f;
            var AB = (Vector3)cell.WorldCenter - arrowTip;
            var middleRight = Vector3.Cross(Vector3.down, AB).normalized * length / 3;
            var middleLeft = Vector3.Cross(AB, Vector3.down).normalized * length / 3;
            Handles.DrawLine(arrowTip, (Vector3)middle + middleRight, thickness);
            Handles.DrawLine(arrowTip, (Vector3)middle + middleLeft, thickness);
        }

        private void DrawCosts(FlowFieldCell cell) {
            Handles.color = Color.magenta;

            var bestCost = "Best cost:" + ((cell.BaseCost == float.MaxValue || cell.BestCost == float.MaxValue) ? "MAX" : (cell.BestCost).ToString("0"));
            var baseCost = "Base cost:" + (cell.BaseCost == float.MaxValue || cell.BestCost == float.MaxValue ? "MAX" : cell.BaseCost.ToString("0"));
            Handles.Label(cell.WorldCenter, bestCost);
            Handles.Label(cell.WorldPosition, baseCost);
        }

        private void DrawSingleCell(FlowFieldCell cell, float cellSize, bool takeHeightIntoAccount = true) {
            var height = takeHeightIntoAccount ? cell.WorldPosition.y : _terrain.transform.position.y;
            cell.WorldPosition.y = height;
            var rightBottom = new Vector3(cell.WorldPosition.x + cellSize, height, cell.WorldPosition.z);
            var rightTop = new Vector3(cell.WorldPosition.x + cellSize, height, cell.WorldPosition.z + cellSize);
            var leftTop = new Vector3(cell.WorldPosition.x, height, cell.WorldPosition.z + cellSize);
            Gizmos.DrawLine(cell.WorldPosition, rightBottom);
            Gizmos.DrawLine(rightBottom, rightTop);
            Gizmos.DrawLine(rightTop, leftTop);
            Gizmos.DrawLine(leftTop, cell.WorldPosition);
        }

        #endregion
    }
}