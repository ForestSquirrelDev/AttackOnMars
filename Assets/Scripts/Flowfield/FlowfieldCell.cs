using System;
using Unity.Mathematics;

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
        
        //debug
        public float3 NormalCenter;
        public float3 NormalEdge;
        public float NormalXWorldUpAngle;

        public bool Equals(FlowfieldCell other) {
            return Index == other.Index;
        }
    }
}