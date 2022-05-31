using Game.Ecs.Flowfield.Components;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Game.Ecs.Flowfield.Systems {
    // Flowfield step 1. Create grid of empty cells.
    public class EmptyCellsGenerationSystem {
        private NativeQueue<EmptyCellsGenerationRequest> _generationRequests;
        private FlowfieldJobDependenciesHandler _jobDependenciesHandler;

        public EmptyCellsGenerationSystem(FlowfieldJobDependenciesHandler dependenciesHandler) {
            _jobDependenciesHandler = dependenciesHandler;
        }

        public void OnCreate() {
            _generationRequests = new NativeQueue<EmptyCellsGenerationRequest>(Allocator.Persistent);
        }
        
        public void OnUpdate() {
            for (int i = _generationRequests.Count - 1; i >= 0; i--) {
                var request = _generationRequests.Dequeue();
                var fillEmptyCellsJob = new FillEmptyCellsJob {
                    CellSize = request.CellSize,
                    GridSize = request.GridSize,
                    Origin = request.Origin,
                    FlowFieldCellsWriter = request.Writer
                };
                _jobDependenciesHandler.ScheduleReadWrite(fillEmptyCellsJob);
            }
        }

        public void OnDestroy() {
            _generationRequests.Dispose();
        }

        public void EnqueueCellsGenerationRequest(EmptyCellsGenerationRequest request) {
            _generationRequests.Enqueue(request);
        }
        
        public struct EmptyCellsGenerationRequest {
            public float CellSize { get; }
            public float3 Origin { get; }
            public int2 GridSize { get; }
            public UnsafeList<FlowfieldCellComponent>.ParallelWriter Writer { get; }

            public EmptyCellsGenerationRequest(float cellSize, float3 origin, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer) {
                CellSize = cellSize;
                Origin = origin;
                GridSize = gridSize;
                Writer = writer;
            }
        }
    }
}
