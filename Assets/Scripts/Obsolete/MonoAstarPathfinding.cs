using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using Utils;

namespace Obsolete {
    /// <summary>
    /// This is obsolete because i realised that pure A* is a poor choice for an RTS game. R.I.P. time. Sad trombone.
    /// </summary>
    public class MonoAstarPathfinding : MonoBehaviour {
        [SerializeField] private Vector2 _from;
        [SerializeField] private Vector2 _to;
        [SerializeField] private Vector2Int _gridSize;

        private void Update() {
            if (Input.GetKeyDown(KeyCode.H)) {
                FindPath(_from, _to);
            }
        }

        private void FindPath(float2 startPos, float2 endPos) {
            int2 gridSize = _gridSize.ToInt2();
            NativeArray<PathNode> pathNodes = new NativeArray<PathNode>((gridSize.x) * (gridSize.y), Allocator.Temp);

            int indexer = 0;
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    float hCost = CalculateEuclideanDistanceHeuristic(new float2(x, y), endPos);
                    float gCost = float.MaxValue / 2;
                    var pathNode = ConstructPathNode(x, y, gCost, hCost, indexer);
                    pathNodes[pathNode.Index] = pathNode;
                    indexer++;
                }
            }
            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);
            
            int startNodeIndex = CalculateIndex(startPos.x, startPos.y);
            int endNodeIndex = CalculateIndex(endPos.x, endPos.y);
            
            PathNode startNode = pathNodes[CalculateIndex(startPos.x, startPos.y, gridSize.x)];
            startNode.GCost = 0;
            startNode.FCost = CalculateFCost(startNode.GCost, startNode.HCost);
            pathNodes[startNode.Index] = startNode;
            
            openList.Add(startNodeIndex);
            
            NativeArray<int2> neighbourOffsets = GetNeighbourOffsets();
            while (openList.Length > 0) {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodes);
                PathNode currentNode = pathNodes[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex) {
                    // reached destination
                    break;
                }
                int indexOfCurrentNode = openList.IndexOf(currentNodeIndex);
                if (indexOfCurrentNode != -1)
                    openList.RemoveAtSwapBack(indexOfCurrentNode);
                closedList.Add(currentNodeIndex);
                
                foreach (var neighbourOffset in neighbourOffsets) {
                    float2 neighbourPosition = new float2(currentNode.X + neighbourOffset.x, currentNode.Y + neighbourOffset.y);
                    if (TileOutOfGrid(neighbourPosition, gridSize)) {
                        continue;
                    }

                    int neighbourIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y);
                    if (closedList.Contains(neighbourIndex)) {
                        continue;
                    }

                    PathNode neighbourNode = pathNodes[neighbourIndex];
                    if (!neighbourNode.IsWalkable) {
                        continue;
                    }

                    float2 currentNodePosition = new float2(currentNode.X, currentNode.Y);
                    float successorGCost = currentNode.GCost + CalculateEuclideanDistanceHeuristic(currentNodePosition, neighbourPosition);
                    Debug.Log($"Tentative g cost: {successorGCost}. Neighbour node g cost: {neighbourNode.GCost}");
                    if (successorGCost < neighbourNode.GCost) {
                        Debug.Log($"Current node index: {currentNodeIndex}");
                        neighbourNode.CameFromNodeIndex = currentNodeIndex;
                        neighbourNode.GCost = successorGCost;
                        neighbourNode.FCost = CalculateFCost(neighbourNode.GCost, neighbourNode.HCost);
                        pathNodes[neighbourIndex] = neighbourNode;

                        if (!openList.Contains(neighbourNode.Index)) {
                            openList.Add(neighbourNode.Index);
                            Debug.Log($"Open list.add");
                        }
                    }
                }
            }
            if (endNodeIndex == -1) {
                Debug.Log($"End node index is minus one");
            } else {
                PathNode endNode = pathNodes[endNodeIndex];
                var path = CalculatePath(pathNodes, endNode, startNode);
                for (var i = 0; i < path.Length; i++) {
                    var pathPos = path[i];
                    Debug.Log($"pathpos: {pathPos}");
                }
                path.Dispose();
                Debug.Log("Disposed");
            }
            
            openList.Dispose();
            closedList.Dispose();
            pathNodes.Dispose();
            neighbourOffsets.Dispose();
        }

        private NativeList<float2> CalculatePath(NativeArray<PathNode> pathNodes, PathNode endNode, PathNode startNode) {
            NativeList<float2> path = new NativeList<float2>(Allocator.Temp);
            if (endNode.CameFromNodeIndex == -1) {
                Debug.Log($"return empty path");
                return path;
            }
            path.Add(new float2(endNode.X, endNode.Y));
            
            PathNode currentNode = endNode;
            while (currentNode.CameFromNodeIndex != -1) {
                PathNode cameFromNode = pathNodes[currentNode.CameFromNodeIndex];
                path.Add(new int2(cameFromNode.X, cameFromNode.Y));
                currentNode = cameFromNode;
            }
            //var pathReversed = path.Reverse(Allocator.Temp);
            Debug.Log("Reversed");
            return path;
        }

        private PathNode ConstructPathNode(int x, int y, float gCost, float hCost, int indexer) {
            PathNode pathNode = new PathNode {
                X = x,
                Y = y,
                Index = CalculateIndex(x, y, indexer),
                GCost = gCost,
                HCost = hCost,
                FCost = CalculateFCost(gCost, hCost),
                IsWalkable = true,
                CameFromNodeIndex = -1
            };
            return pathNode;
        }

        private float CalculateEuclideanDistanceHeuristic(float2 a, float2 b) {
            return math.distance(a, b);
        }
        
        private float CalculateFCost(float gCost, float hCost) {
            return gCost + hCost;
        }

        private int CalculateIndex(float x, float y, int indexer = 0) {
            var max = Mathf.Max(_gridSize.x, _gridSize.y);
            var min = Mathf.Min(_gridSize.x, _gridSize.y);
            var index = (x * _gridSize.x + y) + ((max - min) * x);
            // Debug.Log($"Calculate index. Max: {max}. Min: {min}. x: {x.ToString()}. y: {y.ToString()}. Index: {index}. Indexer: {indexer}");
            return Convert.ToInt32(index);
        }

        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodes) {
            PathNode lowestCostNode = pathNodes[openList[0]];
            for (int i = 1; i < openList.Length; i++) {
                PathNode node = pathNodes[openList[i]];
                if (node.FCost < lowestCostNode.FCost) {
                    lowestCostNode = node;
                }
            }
            return lowestCostNode.Index;
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

        [BurstCompile]
        private struct PathNode {
            public int X;
            public int Y;

            public int Index;
            public int CameFromNodeIndex;

            public float GCost;
            public float HCost;
            public float FCost;

            public bool IsWalkable;

            [BurstDiscard]
            public override string ToString() {
                FixedString512Bytes str = new FixedString512Bytes();
                str.Append($"Position: {X.ToString()}/{Y.ToString()}\n");
                str.Append($"Index: {Index.ToString()}. Came from node index: {CameFromNodeIndex.ToString()} \n");
                str.Append($"GCost: {GCost.ToString("F")}. HCost: {HCost.ToString("F")}. FCost: {FCost.ToString("F")} \n");
                str.Append($"IsWalkable: {IsWalkable.ToString()}");
                return str.Value;
            }
        }
    }
}