using System;
using Game.Ecs.Utils;
using Unity.Mathematics;
#pragma warning disable 660,661

namespace Flowfield {
    [Serializable]
    public struct FlowfieldCell : IEquatable<FlowfieldCell> {
        public float3 WorldPosition;
        public float3 WorldCenter;
        public int2 GridPosition;
        public int Index;
        public float2 Size;
        public float BaseCost;
        public float BestCost;
        public int2 BestDirection;

        public bool Equals(FlowfieldCell other) {
            CompareParameters(this, other, out var indexEquals, out var worldPosEquals, out var bestCostEquals);
            return indexEquals && worldPosEquals && bestCostEquals;
        }

        public override bool Equals(object obj) {
            if (!(obj is FlowfieldCell other)) {
                return false;
            }
            CompareParameters(this, other, out var indexEquals, out var worldPosEquals, out var bestCostEquals);
            return indexEquals && worldPosEquals && bestCostEquals;
        }

        public static bool operator ==(FlowfieldCell cell1, FlowfieldCell cell2) {
            CompareParameters(cell1, cell2, out var indexEquals, out var worldPosEquals, out var bestCostEquals);
            return indexEquals && worldPosEquals && bestCostEquals;
        }

        public static bool operator !=(FlowfieldCell cell1, FlowfieldCell cell2) {
            CompareParameters(cell1, cell2, out var indexEquals, out var worldPosEquals, out var bestCostEquals);
            return !indexEquals || !worldPosEquals || !bestCostEquals;
        }

        private static void CompareParameters(FlowfieldCell cell1, FlowfieldCell cell2, out bool indexEquals, out bool worldPosEquals, out bool bestCostEquals) {
            indexEquals = cell1.Index == cell2.Index;
            worldPosEquals = cell1.WorldPosition.x.Approximately(cell2.WorldPosition.x)
                                      && cell1.WorldPosition.y.Approximately(cell2.WorldPosition.y)
                                      && cell1.WorldPosition.z.Approximately(cell2.WorldPosition.z);
            bestCostEquals = cell1.BestCost.Approximately(cell2.BestCost);
        }
    }
}