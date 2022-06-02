using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Game.Ecs.Flowfield.Components {
    public readonly struct BaseCostAndHeightsGenerationRequest : IComponentData {
        public UnsafeList<FlowfieldCellComponent>.ParallelReader CellsReader { get; }
        public UnsafeList<FlowfieldCellComponent>.ParallelWriter CellsWriter { get; }

        public BaseCostAndHeightsGenerationRequest(UnsafeList<FlowfieldCellComponent>.ParallelReader reader, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer) {
            CellsReader = reader;
            CellsWriter = writer;
        }
        //public NativeQueue<IntegrationFieldGenerationRequest> 
    }
}