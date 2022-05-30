using System.Collections.Generic;
using System.Diagnostics;
using Den.Tools;
using EasyButtons;
using Game.Ecs.Utils;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utils;
using static Flowfield.FlowfieldUtility;
using Debug = UnityEngine.Debug;

namespace Flowfield {
    public readonly struct ChildCellsGenerationRequest {
        public readonly int ParentCellIndex;

        public ChildCellsGenerationRequest(int parentCellIndex) {
            ParentCellIndex = parentCellIndex;
        }
    }

    public readonly struct ChildCellsRemovalRequest {
        public readonly int ParentCellIndex;
        
        public ChildCellsRemovalRequest(int parentCellIndex) {
            ParentCellIndex = parentCellIndex;
        }
    }

    public class FlowfieldController : MonoBehaviour {
        [SerializeField] private int2 _parentGridSizeOut;
        [SerializeField] private float _parentCellSize = 50f;
        [SerializeField] private float _tooBigHeightThreshold = 23f;
        [SerializeField] private float _smallCellSize = 6f;
        [SerializeField] private Terrain _terrain;
        [SerializeField] private float _angleThreshold = 35f;
        [SerializeField] private float _enemySpeed = 30f;
        
        [SerializeField] private Transform _testEnemy;
        
        [SerializeField] private List<FlowFieldCell> _mergedCells = new List<FlowFieldCell>();

        private Queue<ChildCellsGenerationRequest> _childGridGenerationRequests = new Queue<ChildCellsGenerationRequest>();
        private Queue<ChildCellsRemovalRequest> _childCellsRemovalRequests = new Queue<ChildCellsRemovalRequest>();

        private float3 _parentGridOrigin => _terrain.transform.position;
        
        private FlowFieldCell _previousCell = FlowFieldCell.Null;
        private Vector3 _targetWorld;
        private FlowFieldCell _targetCell;

        private void Update() {
            if (Input.GetKeyDown(KeyCode.F)) {
                var world = InputUtility.MouseToWorld(Camera.main, true);
                _targetWorld = world;
                GenerateMergedGrid(_parentCellSize);
                var targetCell = _mergedCells[CalculateIndexFromWorld(world, _parentGridOrigin, _parentGridSizeOut, _parentCellSize)];
                CreateIntegrationField(targetCell, _parentGridSizeOut, _mergedCells);
                CreateFlowField(_mergedCells, _parentGridSizeOut, world);
                _targetCell = targetCell;
            }
            if (Input.GetKeyDown(KeyCode.H)) {
                var world = InputUtility.MouseToWorld(Camera.main, true);
                var index = CalculateIndexFromWorld(world, _parentGridOrigin, _parentGridSizeOut, _parentCellSize);
                var cell = _mergedCells[index];
                GenerateChildGrid(world, cell, _targetCell, _smallCellSize, _mergedCells, _parentGridSizeOut);
            }
            FindPath(_targetWorld, _mergedCells, _testEnemy, _parentGridOrigin, _parentCellSize, _parentGridSizeOut);
        }

        private void FixedUpdate() {
            if (_targetWorld == default) return;
            EnqueueChildCellsGenerationRequest(_testEnemy.position, _parentGridOrigin, _parentGridSizeOut, _parentCellSize);
            ManageChildCellsGenerationRequests(_targetWorld, _smallCellSize, _childGridGenerationRequests, _mergedCells, _parentGridSizeOut);

            var currentEnemyCell = _mergedCells[CalculateIndexFromWorld(_testEnemy.position, _parentGridOrigin, _parentGridSizeOut, _parentCellSize)];
            SeeIfNeedClearGrid(currentEnemyCell, _parentGridSizeOut);
            ManageClearGridRequests(_childCellsRemovalRequests, _mergedCells);
            _previousCell = currentEnemyCell;
        }

        private void SeeIfNeedClearGrid(FlowFieldCell currentCell, int2 parentGridSize) {
            if (_previousCell == FlowFieldCell.Null) return;
            if (currentCell != _previousCell) {
                var parentCellIndex = CalculateIndexFromGrid(_previousCell.GridPosition, parentGridSize);
                _childCellsRemovalRequests.Enqueue(new ChildCellsRemovalRequest(parentCellIndex));
            }
        }

        private void ManageClearGridRequests(Queue<ChildCellsRemovalRequest> allRequests, IList<FlowFieldCell> parentCells) {
            if (allRequests.Count == 0) return;

            var request = allRequests.Dequeue();
            parentCells[request.ParentCellIndex].ChildCells.Clear();
        }

        private void EnqueueChildCellsGenerationRequest(Vector3 enemyPosition, float3 parentGridOrigin, int2 parentGridSize, float parentCellSize) {
            var parentCellIndex = CalculateIndexFromWorld(enemyPosition, parentGridOrigin, parentGridSize, parentCellSize);
            _childGridGenerationRequests.Enqueue(new ChildCellsGenerationRequest(parentCellIndex));
        }

        private void ManageChildCellsGenerationRequests(Vector3 targetWorldPosition, float childCellSize, Queue<ChildCellsGenerationRequest> requests, IList<FlowFieldCell> parentCells, int2 parentGridSize) {
            if (requests.Count == 0) return;

            for (int i = requests.Count - 1; i > 0; i--) {
                var request = requests.Dequeue();
                var requestedParentCell = parentCells[request.ParentCellIndex];
                if (requestedParentCell.ChildCells != null && requestedParentCell.ChildCells.Count > 0) continue;

                var targetParentCell = parentCells[CalculateIndexFromWorld(targetWorldPosition, _parentGridOrigin, _parentGridSizeOut, _parentCellSize)];
                GenerateChildGrid(targetWorldPosition, requestedParentCell, targetParentCell, childCellSize, parentCells, parentGridSize);
            }
        }

        private FlowFieldCell FindClosestCellToNextBestCell(FlowFieldCell bestDirectionParentCell, FlowFieldCell parentCell, IList<FlowFieldCell> childCells, int2 childCellsGridSize, float childCellSize) {
            var directionFromGridOriginToBestParentCell = math.normalize(bestDirectionParentCell.WorldCenter - parentCell.WorldCenter)* childCellSize;
            FlowFieldCell bestChildCell = new FlowFieldCell();
            float3 bestCellWorldPosition = parentCell.WorldCenter;
            
            var worldOutOfGrid = WorldOutOfGrid(bestCellWorldPosition, parentCell.WorldRect);
            while (!worldOutOfGrid) {
                var bestCellIndex = CalculateIndexFromWorld(bestCellWorldPosition, parentCell.WorldPosition, childCellsGridSize, childCellSize);
                bestChildCell = childCells[bestCellIndex];
                
                bestCellWorldPosition += (float3)directionFromGridOriginToBestParentCell;
                worldOutOfGrid = WorldOutOfGrid(bestCellWorldPosition, parentCell.WorldRect);
            }

            return bestChildCell;
        }

        private void FindPath(Vector3 target, IList<FlowFieldCell> allCells, Transform enemy, float3 gridOrigin, float cellSize, int2 gridSize) {
            if (target == default) return;
            var enemyPos = enemy.position;
            var arrived = enemyPos.Approximately(target);
            if (!arrived) {
                var currentCellIndex = CalculateIndexFromWorld(enemyPos, gridOrigin, gridSize, cellSize);
                var currentCell = allCells[currentCellIndex];
                var index = CalculateIndexFromWorld(enemyPos, currentCell.WorldPosition, currentCell.ChildGridSize, _smallCellSize);
                Debug.Log($"Index of child cell: {index}");
                if (currentCell.ChildCells == null || currentCell.ChildCells.Count == 0) return;
                var currentChildCell = currentCell.ChildCells[index];
                var bestDirection = math.normalize(currentChildCell.BestDirection);
                var worldDirection = new Vector3(bestDirection.x, 0, bestDirection.y);
                worldDirection *= _enemySpeed;
                
                var position = enemy.position;
                position += worldDirection * Time.deltaTime;
                var height = SampleHeightRaycast(position) + 7f;
                position = new Vector3(position.x, height, position.z);
                enemy.position = position;
            }
        }

        [Button]
        private void GenerateChildGrid(Vector3 target, FlowFieldCell parentCell, FlowFieldCell targetParentCell, float childCellSize, IList<FlowFieldCell> parentCells, int2 parentCellsGridSize) {
            var gridSize = new int2(Mathf.FloorToInt(parentCell.WorldRect.Width / childCellSize),
                Mathf.FloorToInt(parentCell.WorldRect.Height / childCellSize));
            var origin = parentCell.WorldPosition;
            parentCell.ChildCells = new List<FlowFieldCell>(gridSize.x * gridSize.y);
            parentCell.ChildGridSize = new int2(Mathf.FloorToInt(gridSize.x), Mathf.FloorToInt(gridSize.y));
            var cells = parentCell.ChildCells;
            FillEmptyCells(childCellSize, origin, gridSize, cells);
            
            var bestParentPosition = parentCell.GridPosition + parentCell.BestDirection;
            var bestParentIndex = CalculateIndexFromGrid(bestParentPosition, parentCellsGridSize);
            if (IsOutOfRange(parentCells, bestParentIndex)) {
                Debug.LogError($"Failed to generate child grid: Index out of range for best direction parent cell. Index: {bestParentIndex}. Count: {parentCells.Count}");
                return;
            }
            var bestCell = parentCell != targetParentCell 
                ? FindClosestCellToNextBestCell(parentCells[bestParentIndex], parentCell, parentCell.ChildCells, gridSize, childCellSize)
                : parentCell.ChildCells[CalculateIndexFromWorld(target, parentCell.WorldPosition, parentCell.ChildGridSize, childCellSize)];
            bestCell.IsBestChildCell = true;
            CreateIntegrationField(bestCell, gridSize, parentCell.ChildCells);
            CreateFlowField(parentCell.ChildCells, gridSize, target);
            
            Debug.Log($"Parent cell: {parentCell.GridPosition}. Best small cell: {bestCell.GridPosition}. ");

            parentCells[CalculateIndexFromGrid(parentCell.GridPosition, parentCellsGridSize)] = parentCell;
        }

        [Button]
        private void GenerateMergedGrid(float cellSize) {
            var terrain = _terrain;
            var terrainRect = terrain.GetWorldRect();
            Debug.Log($"{terrainRect}");
            var origin = terrain.transform.position;
            var w = Mathf.FloorToInt(terrainRect.width / cellSize);
            var h = Mathf.FloorToInt(terrainRect.height / cellSize);
            var gridSize = new int2(w, h);
            _mergedCells = new List<FlowFieldCell>(gridSize.x * gridSize.y);
            FillEmptyCells(cellSize, origin, gridSize, _mergedCells);
            _parentGridSizeOut = gridSize;
        }
        
        private void FillEmptyCells(float cellSize, Vector3 origin, int2 gridSize, IList<FlowFieldCell> cells) {
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    var cell = new FlowFieldCell();
                    cell.GridPosition = new int2(x, y);
                    cell.WorldPosition = ToWorld(cell.GridPosition, origin, cellSize);
                    cell.WorldPosition.y = _terrain.SampleHeight(cell.WorldPosition);
                    cell.WorldCenter = FindCellCenter(cell.WorldPosition, cellSize);
                    cell.WorldCenter.y = _terrain.SampleHeight(cell.WorldCenter);
                    cell.GridPosition = new int2(x, y);
                    var cellRect = new FlowFieldRect {
                        X = cell.WorldPosition.x,
                        Y = cell.WorldPosition.z,
                        Height = (int)cellSize,
                        Width = (int)cellSize
                    };
                    cell.Size = cellSize;
                    cell.WorldRect = cellRect;
                    cell.BaseCost = FindBaseCost(cell.WorldCenter, cell.WorldPosition);
                    cell.BestCost = float.MaxValue;

                    cells.Add(cell);
                }
            }
        }

        private void CreateIntegrationField(FlowFieldCell targetCell, int2 gridSize, IList<FlowFieldCell> allCells) {
            if (TileOutOfGrid(targetCell.GridPosition, gridSize)) {
                return;
            }
            var openList = new Queue<FlowFieldCell>();
            var closedList = new List<FlowFieldCell>();
            targetCell.BaseCost = 0;
            targetCell.BestCost = 0;
            allCells[CalculateIndexFromGrid(targetCell.GridPosition, gridSize)] = targetCell;
            openList.Enqueue(targetCell);

            while (openList.Count > 0) {
                var currentCell = openList.Dequeue();
                var neighbours = FindNeighbours(currentCell, allCells, gridSize);
                for (var i = 0; i < neighbours.Count; i++) {
                    var neighbour = neighbours[i];
                    if (TileOutOfGrid(neighbour.GridPosition, gridSize) 
                        || neighbour.BaseCost == float.MaxValue || closedList.Contains(neighbour)) {
                        continue;
                    }
                    var totalCost = neighbour.BaseCost + currentCell.BestCost;
                    if (totalCost < neighbour.BestCost) {
                        neighbour.BestCost = totalCost;
                        var neighbourIndex = CalculateIndexFromGrid(neighbour.GridPosition, gridSize);
                        allCells[neighbourIndex] = neighbour;
                        openList.Enqueue(neighbour);
                        closedList.Add(neighbour);
                    }
                }
            }
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

        private void CreateFlowField(IList<FlowFieldCell> allCells, int2 gridSize, Vector3 targetWorldPosition) {
            for (var i = 0; i < allCells.Count; i++) {
                var cell = allCells[i];
                var index = CalculateIndexFromGrid(cell.GridPosition, gridSize);
                
                if (cell.BaseCost == 0 && cell.BestCost == 0) {
                    var worldDirection = (targetWorldPosition - (Vector3)cell.WorldCenter).normalized;
                    cell.BestDirection = new int2(Mathf.RoundToInt(worldDirection.x), Mathf.RoundToInt(worldDirection.z));
                } else {
                    var neighbours = FindNeighbours(cell, allCells, gridSize);
                    var bestDirection = FindBestDirectionBasedOnCosts(cell, neighbours);
                    cell.BestDirection = bestDirection;
                }
                
                allCells[index] = cell;
            }
        }

        private int2 FindBestDirectionBasedOnCosts(FlowFieldCell currentCell, List<FlowFieldCell> validNeighbours) {
            var bestCell = FindLowestCostCellSlow(validNeighbours);
            return bestCell.GridPosition - currentCell.GridPosition;
        }

        private FlowFieldCell FindLowestCostCellSlow(List<FlowFieldCell> validNeighbours) {
            var bestCell = new FlowFieldCell {
                BestCost = float.MaxValue,
                GridPosition = new int2(-1, -1)
            };
            for (var i = 0; i < validNeighbours.Count; i++) {
                var currentCell = validNeighbours[i];
                
                if (currentCell.BestCost < bestCell.BestCost) {
                    bestCell = currentCell;
                }
            }
            if (bestCell.GridPosition.x == -1) {
                //Debug.LogError("Couldn't find best cell");
            }
            return bestCell;
        }

        private List<FlowFieldCell> FindNeighbours(FlowFieldCell currentCell, IList<FlowFieldCell> allCells, int2 gridSize) {
            var neighbourOffsets = GetNeighbourOffsets();
            var neighbours = new List<FlowFieldCell>();

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
    }
}