using Unity.Mathematics;

namespace Game.Ecs.Flowfield.Systems {
    public readonly struct FlowfieldRuntimeData {
        public float3 ParentGridOrigin { get; }
        public int2 ParentGridSize { get; }
        public int2 ChildGridSize { get; }
        public float ParentCellSize { get; }
        public float ChildCellSize { get; }
        public float UnwalkableAngleThreshold { get; }
        public float CostlyHeightThreshold { get; }

        public FlowfieldRuntimeData(float3 parentGridOrigin, int2 parentGridSize, int2 childGridSize, float parentCellSize, float childCellSize,
            float unwalkableAngleThreshold, float costlyHeightThreshold) {
            ParentGridOrigin = parentGridOrigin;
            ParentGridSize = parentGridSize;
            ParentCellSize = parentCellSize;
            ChildCellSize = childCellSize;
            ChildGridSize = childGridSize;
            UnwalkableAngleThreshold = unwalkableAngleThreshold;
            CostlyHeightThreshold = costlyHeightThreshold;
        }
    }
}
