using Game.Ecs.Components.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Utils.Pathfinding;
using ChildCellsGenerationRequest = Game.Ecs.Systems.Pathfinding.ChildCellsGenerationRequest;

namespace Game.Ecs.Systems.Pathfinding {
    [UpdateAfter(typeof(FlowfieldManagerSystem))]
    public partial class DetectEnemiesAndScheduleChildCellsSystem : SystemBase {
        private DependenciesScheduler _dependenciesScheduler;
        private FlowfieldRuntimeData _flowfieldRuntimeData;
        private ManageChildCellsGenerationRequestsSystem _childCellsGenerationSubsystem;

        public void Construct(DependenciesScheduler scheduler, FlowfieldRuntimeData flowfieldRuntimeData, ManageChildCellsGenerationRequestsSystem childCellsGenerationSubsystem) {
            _dependenciesScheduler = scheduler;
            _flowfieldRuntimeData = flowfieldRuntimeData;
            _childCellsGenerationSubsystem = childCellsGenerationSubsystem;
        }
        
        protected override void OnUpdate() {
            if (_dependenciesScheduler == null) return;
            var inputDeps = _dependenciesScheduler.GetCombinedReadWriteDependencies();
            var inputDepsCombined = JobHandle.CombineDependencies(inputDeps, Dependency);
            var gridOrigin = _flowfieldRuntimeData.ParentGridOrigin;
            var gridSize = _flowfieldRuntimeData.ParentGridSize;
            var cellSize = _flowfieldRuntimeData.ParentCellSize;
            var requestsIn = new NativeHashMap<int2, ChildCellsGenerationRequest>(gridSize.x * gridSize.y, Allocator.TempJob);
            var requestsOut = _childCellsGenerationSubsystem.Requests;
            
            var handle = Entities.WithAll<Tag_Enemy>().ForEach((in LocalToWorld ltw) => {
                var gridPos = FlowfieldUtility.ToGrid(ltw.Position, gridOrigin, cellSize);
                if (FlowfieldUtility.TileOutOfGrid(gridPos, gridSize)) return;
                
                requestsIn[gridPos] = new ChildCellsGenerationRequest(gridPos, 1);
            }).Schedule(inputDepsCombined);
            
            var scheduleRequestsJob = new ScheduleGenerationRequestsJob(requestsIn, requestsOut).Schedule(handle);
            var combinedDeps = JobHandle.CombineDependencies(handle, scheduleRequestsJob);
            
            _dependenciesScheduler.AddExternalReadWriteDependency(combinedDeps);
            Dependency = combinedDeps;
            requestsIn.Dispose(combinedDeps);
        }
        
        [BurstCompile]
        private struct ScheduleGenerationRequestsJob : IJob {
            private NativeHashMap<int2, ChildCellsGenerationRequest> _requestsIn;
            private NativeHashMap<int2, ChildCellsGenerationRequest> _requestsOut;

            public ScheduleGenerationRequestsJob(NativeHashMap<int2, ChildCellsGenerationRequest> requestsIn, NativeHashMap<int2, ChildCellsGenerationRequest> requestsOut) {
                _requestsIn = requestsIn;
                _requestsOut = requestsOut;
            }
            
            public void Execute() {
                foreach (var request in _requestsIn) {
                    var persistentRequest = _requestsOut[request.Key];
                    persistentRequest.IncrementLifetime();
                    _requestsOut[request.Key] = persistentRequest;
                }
            }
        }
    }
}
