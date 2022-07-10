using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Utils.Maths;
using Utils.Pathfinding;

#pragma warning disable 660,661

namespace Obsolete {
    [Serializable]
    public struct FlowFieldCell : IEquatable<FlowFieldCell> {
        public float3 WorldPosition;
        public float3 WorldCenter;
        public int2 GridPosition;
        public List<FlowFieldCell> ChildCells;
        public int2 ChildGridSize;
        public float2 Size;
        public FlowFieldRect WorldRect;
        public float BaseCost;
        public float BestCost;
        public int2 BestDirection;
        
        // for gizmos
        public bool IsBestChildCell;

        public bool Equals(FlowFieldCell other) {
            CompareParameters(this, other, out var gridPosEquals, out var worldPosEquals, out var bestCostEquals);
            return gridPosEquals && worldPosEquals && bestCostEquals;
        }

        public static bool operator ==(FlowFieldCell cell1, FlowFieldCell cell2) {
            CompareParameters(cell1, cell2, out var gridPosEquals, out var worldPosEquals, out var bestCostEquals);
            return gridPosEquals && worldPosEquals && bestCostEquals;
        }

        public static bool operator !=(FlowFieldCell cell1, FlowFieldCell cell2) {
            CompareParameters(cell1, cell2, out var gridPosEquals, out var worldPosEquals, out var bestCostEquals);
            return !gridPosEquals || !worldPosEquals || !bestCostEquals;
        }

        public override string ToString() {
            return $"{GridPosition.ToString()}";
        }

        private static void CompareParameters(FlowFieldCell cell1, FlowFieldCell cell2, out bool gridPosEquals, out bool worldPosEquals, out bool bestCostEquals) {
            gridPosEquals = cell1.GridPosition.x == cell2.GridPosition.x && cell1.GridPosition.y == cell2.GridPosition.y;
            worldPosEquals = cell1.WorldPosition.x.Approximately(cell2.WorldPosition.x)
                             && cell1.WorldPosition.y.Approximately(cell2.WorldPosition.y)
                             && cell1.WorldPosition.z.Approximately(cell2.WorldPosition.z);
            bestCostEquals = cell1.BestCost.Approximately(cell2.BestCost);
        }

        public static FlowFieldCell Null => new FlowFieldCell();
    }
}