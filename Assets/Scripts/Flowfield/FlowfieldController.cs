using System;
using System.Diagnostics;
using Den.Tools;
using EasyButtons;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Maths;
using Debug = UnityEngine.Debug;

namespace Flowfield {
    public class FlowfieldController : MonoBehaviour {
        [SerializeField] private int2 _gridSize;
        [SerializeField] private int2 _parentGridSizeOut;
        [SerializeField] private float _parentCellSize = 50f;
        [SerializeField] private float _tooBigHeightThreshold = 23f;
        [SerializeField] private float _cellSize;
        [SerializeField] private FlowfieldCell[] _allCells;
        [SerializeField] private Terrain _terrain;
        [SerializeField] private float _angleThreshold = 35f;
        
        [SerializeField] private bool _debugNormals = true;
        [SerializeField] private bool _debugCosts = true;
        [SerializeField] private bool _drawArrows = true;
        [SerializeField] private bool _debugPositions;
        [SerializeField] private bool _debugParentGrid = true;
        [SerializeField] private Transform _testEnemy;

        private float3 _origin => transform.position;

        private Stopwatch _sw = new Stopwatch();
        private FlowfieldCell _currentCell;
        
        private FlowfieldCell _targetCell;
        
        private void Update() {
            if (Input.GetKeyDown(KeyCode.F)) {
                var world = InputUtility.MouseToWorld(Camera.main, true);
                GenerateParentGrid(_parentCellSize, out _parentGridSizeOut);
                InitializeGrid();
                CreateIntegrationField(world);
                CreateFlowField(_allCells);
            }
            if (_targetCell == default) return;
            var enemyGridPosition = FlowfieldUtility.ToGrid(_testEnemy.position, _origin, _cellSize);
            var hasNotArrived = enemyGridPosition.x != _targetCell.GridPosition.x && enemyGridPosition.y != _targetCell.GridPosition.y;
            //Debug.Log($"Has not arrived: {hasNotArrived}");
            if (hasNotArrived) {
                var currentCellIndex = FlowfieldUtility.CalculateIndexFromGrid(enemyGridPosition, _gridSize);
                if (currentCellIndex >= _allCells.Length || currentCellIndex < 0) {
                    //Debug.Log($"Current cell index out of range: {currentCellIndex}.");
                    return;
                }
                var currentCell = _allCells[currentCellIndex];
                var bestDirection = math.normalize(currentCell.BestDirection);
                var worldDirection = new Vector3(bestDirection.x, 0, bestDirection.y);
                _testEnemy.position = _testEnemy.position + worldDirection * 6f * Time.deltaTime;
                //Debug.Log($"Enemy grid pos: {currentCellIndex}. Current cell: {currentCell.GridPosition}. Best direction: {bestDirection}. World direction: {worldDirection}");
            }
        }

        [SerializeField] private FlowfieldCell[] _parentCells;

        private void GenerateParentGrid(float cellSize, out int2 parentGridSize) {
            var terrain = _terrain;
            var rect = terrain.GetWorldRect();
            var origin = terrain.transform.position;
            var w = Mathf.FloorToInt(rect.width / cellSize);
            var h = Mathf.FloorToInt(rect.height / cellSize);
            var gridSize = new int2(w, h);
            _parentCells = new FlowfieldCell[w * h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++) {
                    var cell = new FlowfieldCell();
                    cell.GridPosition = new int2(x, y);
                    var index = FlowfieldUtility.CalculateIndexFromGrid(x, y, gridSize);
                    cell.WorldPosition = FlowfieldUtility.ToWorld(cell.GridPosition, origin, cellSize);
                    cell.WorldPosition.y = _terrain.SampleHeight(cell.WorldPosition);
                    cell.WorldCenter = FlowfieldUtility.FindCellCenter(cell.WorldPosition, cellSize);
                    cell.WorldCenter.y = _terrain.SampleHeight(cell.WorldCenter);
                    _parentCells[index] = cell;
                }
            parentGridSize = gridSize;
            Debug.Log($"w: {w}. h: {h}");
        }

        [Button]
        public void InitializeGrid() {
            _sw.Start();
            _allCells = new FlowfieldCell[_gridSize.x * _gridSize.y];
            int i = 0;
            var origin = transform.position;
            for (int x = 0; x < _gridSize.x; x++) {
                for (int z = 0; z < _gridSize.y; z++) {
                    var flowFieldCell = new FlowfieldCell();
                    var posX = origin.x + x * _cellSize;
                    var posZ = origin.z + z * _cellSize;
                    var posY = _terrain.SampleHeight(new Vector3(posX, origin.y, posZ));
                    var position = new float3(posX, posY, posZ);
                    var center = FlowfieldUtility.FindCellCenter(new float3(posX, posY, posZ), _cellSize);
                    
                    flowFieldCell.WorldPosition = position;
                    flowFieldCell.Index = FlowfieldUtility.CalculateIndexFromGrid(x, z, _gridSize);
                    flowFieldCell.WorldCenter = center;
                    flowFieldCell.Size = _cellSize;
                    flowFieldCell.GridPosition = new int2(x, z);
                    flowFieldCell.BestCost = float.MaxValue;
                    
                    flowFieldCell = FindBaseCost(center, posX, posY, posZ, flowFieldCell);
                    _allCells[i] = flowFieldCell;
                    i++;
                }
            }
            _sw.Stop();
            Debug.Log($"Elapsed miliseconds: {_sw.ElapsedMilliseconds}");
            _sw.Reset();
        }

        private void CreateIntegrationField(Vector3 end) {
            _sw.Start();
            var endGridPos = FlowfieldUtility.ToGrid(end, _origin, _cellSize);
            if (FlowfieldUtility.TileOutOfGrid(endGridPos, _gridSize)) {
                return;
            }
            var openList = new NativeQueue<FlowfieldCell>(Allocator.Temp);
            var closedList = new NativeList<FlowfieldCell>(Allocator.Temp);
            var index = FlowfieldUtility.CalculateIndexFromWorld(end.x, end.z, _origin, _gridSize, _cellSize);
            var endCell = _allCells[index];
            //Debug.Log($"End cell: {endCell.GridPosition}. Index: {endCell.Index}");
            endCell.BaseCost = 0;
            endCell.BestCost = 0;
            _allCells[endCell.Index] = endCell;
            openList.Enqueue(endCell);

            while (openList.Count > 0) {
                var currentCell = openList.Dequeue();
                var neighbours = FindNeighbours(currentCell, _allCells);
                for (var i = 0; i < neighbours.Length; i++) {
                    var neighbour = neighbours[i];
                    if (FlowfieldUtility.TileOutOfGrid(neighbour.GridPosition, _gridSize) 
                        || neighbour.BaseCost == float.MaxValue || closedList.Contains(neighbour)) {
                        continue;
                    }
                    var totalCost = neighbour.BaseCost + currentCell.BestCost;
                    if (totalCost < neighbour.BestCost) {
                        neighbour.BestCost = totalCost;
                        _allCells[neighbour.Index] = neighbour;
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
                    baseCost = 1f;
                    if (flowFieldCell.WorldCenter.y > _tooBigHeightThreshold) {
                        baseCost += hitCenter.point.y;
                    }
                }
                flowFieldCell.BaseCost = baseCost;
            }
            return flowFieldCell;
        }

        private void CreateFlowField(FlowfieldCell[] allCells) {
            // нам нужно найти клетку, разница в высоте с которой у нашей клетки самая маленькая. то есть нам нужно стремиться идти по максимально ровной поверхности.
            // сейчас же выбор идёт просто по наименьшей высоте
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
            return bestCell.GridPosition - currentCell.GridPosition;
        }

        private FlowfieldCell FindLowestCostCellSlow(NativeArray<FlowfieldCell> validNeighbours) {
            var bestCell = new FlowfieldCell {
                BestCost = float.MaxValue,
                Index = -1
            };
            
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

        private NativeList<FlowfieldCell> FindNeighbours(FlowfieldCell currentCell, FlowfieldCell[] allCells) {
            var neighbourOffsets = FlowfieldUtility.GetNeighbourOffsets();
            var neighbours = new NativeList<FlowfieldCell>(Allocator.Temp);

            for (var i = 0; i < neighbourOffsets.Length; i++) {
                var neighbourOffset = neighbourOffsets[i];
                var neighbourGridPosition = new int2(currentCell.GridPosition.x + neighbourOffset.x, currentCell.GridPosition.y + neighbourOffset.y);
                if (FlowfieldUtility.TileOutOfGrid(neighbourGridPosition, _gridSize))
                    continue;
                var neighbourIndex = FlowfieldUtility.CalculateIndexFromGrid(neighbourGridPosition.x, neighbourGridPosition.y, _gridSize);
                var neighbourCell = allCells[neighbourIndex];
                neighbours.Add(neighbourCell);
            }

            neighbourOffsets.Dispose();
            return neighbours;
        }
        #region gizmo

        private void OnDrawGizmos() {
            if (_allCells == null) return;
            foreach (var cell in _allCells) {
                Gizmos.color = cell.BestCost == float.MaxValue ? Color.red : Color.green;
                DrawSingleCell(cell, _cellSize);

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
                    Handles.color = Color.magenta;

                    var text = (cell.BaseCost == float.MaxValue || cell.BestCost == float.MaxValue) ? "MAX" : (cell.BestCost).ToString("0");
                    Handles.Label(cell.WorldCenter, text);
                }

                if (_debugPositions) {
                    var text = cell.GridPosition.ToString();
                    Handles.Label(cell.WorldPosition, text);
                }

                if (_drawArrows && !(cell.BestDirection.x == 0 && cell.BestDirection.y == 0) && cell.BestCost != float.MaxValue) {
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
            }
            
            if (_debugParentGrid) {
                foreach (var cell in _parentCells) {
                    Gizmos.color = Color.white;;
                    DrawSingleCell(cell, 50);
                }       
            }
        }
        
        private void DrawSingleCell(FlowfieldCell cell, float cellSize) {
            var rightBottom = new Vector3(cell.WorldPosition.x + cellSize, cell.WorldPosition.y, cell.WorldPosition.z);
            var rightTop = new Vector3(cell.WorldPosition.x + cellSize, cell.WorldPosition.y, cell.WorldPosition.z + cellSize);
            var leftTop = new Vector3(cell.WorldPosition.x, cell.WorldPosition.y, cell.WorldPosition.z + cellSize);
            Gizmos.DrawLine(cell.WorldPosition, rightBottom);
            Gizmos.DrawLine(rightBottom, rightTop);
            Gizmos.DrawLine(rightTop, leftTop);
            Gizmos.DrawLine(leftTop, cell.WorldPosition);
        }

        #endregion
    }
}