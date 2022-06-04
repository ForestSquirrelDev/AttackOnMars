using System.Threading.Tasks;
using Game.Ecs.Flowfield.Components;
using Game.Ecs.Flowfield.Configs;
using Game.Ecs.Systems.Spawners;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;

namespace Game.Ecs.Flowfield.Systems {
    public class ManageChildCellsGenerationRequestsSystem {
        public readonly struct ChildCellsGenerationRequest {
            public readonly float3 World;

            public ChildCellsGenerationRequest(float3 world) {
                World = world;
            }
        }

        public NativeQueue<ChildCellsGenerationRequest> Requests;

        private UnsafeList<FlowfieldCellComponent> _parentCells;
        private FlowfieldJobDependenciesHandler _jobDependenciesHandler;

        public void Init(FlowfieldJobDependenciesHandler dependenciesHandler, UnsafeList<FlowfieldCellComponent> parentCells) {
            _jobDependenciesHandler = dependenciesHandler;
            _parentCells = parentCells;
        }

        public void OnUpdate() {
            
        }
    }
    // Flowfield level 0.5. Initialize grid systems and schedule creation of empty parent grid.
    public partial class FlowfieldManagerSystem : SystemBase {
        public bool Initialized { get; private set; }
        
        public UnsafeList<FlowfieldCellComponent> ParentFlowFieldCells;
        public NativeList<UnsafeList<FlowfieldCellComponent>> ChildCells;
        
        private FlowfieldJobDependenciesHandler _jobDependenciesHandler;
        private EmptyCellsGenerationSubSystem _emptyCellsGenerationSubSystem;
        private FindBaseCostAndHeightsSubSystem _findBaseCostAndHeightsSubSystem;
        private GenerateIntegrationFieldSubsystem _generateIntegrationFieldSubsystem;
        private GenerateFlowFieldSubsystem _generateFlowFieldSubsystem;

        private FlowfieldConfig _flowfieldConfig;
        private TerrainData _terrainData;

        protected override void OnCreate() {
            _jobDependenciesHandler = new FlowfieldJobDependenciesHandler();
            _findBaseCostAndHeightsSubSystem = new FindBaseCostAndHeightsSubSystem(_jobDependenciesHandler);
            _emptyCellsGenerationSubSystem = new EmptyCellsGenerationSubSystem(_jobDependenciesHandler, _findBaseCostAndHeightsSubSystem);
            _generateIntegrationFieldSubsystem = new GenerateIntegrationFieldSubsystem(_jobDependenciesHandler);
            _generateFlowFieldSubsystem = new GenerateFlowFieldSubsystem(_jobDependenciesHandler);
            
            _jobDependenciesHandler.OnCreate();
        }

        public async void Awake(Transform terrainTransform) {
            _flowfieldConfig = await LoadFlowfieldConfig();
            _terrainData = await LoadTerrainData();
            var terrainPosition = terrainTransform.position;
            var parentGridSize = FindParentGridSize(terrainPosition, _terrainData, _flowfieldConfig);
            Debug.Log($"Parent grid size: {parentGridSize}. Capacity: {parentGridSize.x * parentGridSize.y}");
            
            ParentFlowFieldCells = new UnsafeList<FlowfieldCellComponent>(parentGridSize.x * parentGridSize.y, Allocator.Persistent);
            ChildCells = new NativeList<UnsafeList<FlowfieldCellComponent>>(ParentFlowFieldCells.Length, Allocator.Persistent);
            
            unsafe {
                Debug.Log($"Created list and scheduled jobs. List capacity: {ParentFlowFieldCells.AsParallelWriter().ListData->m_capacity}");
            }
            
            var fillEmptyCellsJob = _emptyCellsGenerationSubSystem.Schedule(_flowfieldConfig.ParentCellSize, parentGridSize, terrainPosition, ParentFlowFieldCells.AsParallelWriter(), default(JobHandle));
            var fillHeightsJob = _findBaseCostAndHeightsSubSystem.Schedule(ParentFlowFieldCells.AsParallelWriter(), parentGridSize, fillEmptyCellsJob, 
                _flowfieldConfig.UnwalkableAngleThreshold, _flowfieldConfig.CostHeightThreshold);
            var createChildListsJob = new CreateChildNativeListsJob(ParentFlowFieldCells.AsParallelWriter(), _flowfieldConfig.ChildCellSize);
            var createListsHandle = _jobDependenciesHandler.ScheduleReadWrite(createChildListsJob, dependenciesIn: fillHeightsJob);
            var generateIntegrationFieldJob = _generateIntegrationFieldSubsystem.Schedule(MonoHivemind.Instance.CurrentTarget, parentGridSize, ParentFlowFieldCells.AsParallelWriter(), createListsHandle);
            var generateFlowfieldJob = _generateFlowFieldSubsystem.Schedule(ParentFlowFieldCells.AsParallelWriter(), parentGridSize, MonoHivemind.Instance.CurrentTarget, generateIntegrationFieldJob);

            Initialized = true;
        }

        protected override void OnUpdate() {
            _jobDependenciesHandler.OnUpdate();

            if (Input.GetKeyDown(KeyCode.I)) {
                ShowDebugInfo();
            }
        }

        protected override void OnDestroy() {
            _jobDependenciesHandler.OnDestroy();
            ParentFlowFieldCells.Dispose();
            ChildCells.Dispose();
            foreach (var cell in ParentFlowFieldCells) {
                cell.Dispose();
            }
            Debug.Log($"Parent cells count: {ParentFlowFieldCells.Length} Is created: {ParentFlowFieldCells.IsCreated}");
        }
        
        public JobHandle ScheduleReadWrite<T>(T flowfieldRelatedJob, int framesLifetime = 4) where T: struct, IJob {
            return _jobDependenciesHandler.ScheduleReadWrite(flowfieldRelatedJob, framesLifetime);
        }
        
        public JobHandle ScheduleReadOnly<T>(T flowfieldRelatedJob, int framesLifetime = 1) where T: struct, IJob {
            return _jobDependenciesHandler.ScheduleReadOnly(flowfieldRelatedJob, framesLifetime);
        }

        public void CompleteAll() {
            _jobDependenciesHandler.CompleteAll();
        }

        private async Task<FlowfieldConfig> LoadFlowfieldConfig() {
            var flowfieldConfigHandle = Addressables.LoadAssetAsync<FlowfieldConfig>("config_flowfieldConfig");
            await flowfieldConfigHandle.Task;
            return flowfieldConfigHandle.Result;
        }
        
        private static async Task<TerrainData> LoadTerrainData() {
            var terrainDataHandle = Addressables.LoadAssetAsync<TerrainData>("config_defaultTerrainData");
            await terrainDataHandle.Task;
            var terrainData = terrainDataHandle.Result;
            return terrainData;
        }
        
        private int2 FindParentGridSize(Vector3 terrainPosition, TerrainData terrainData, FlowfieldConfig config) {
            var terrainRect = TerrainUtility.GetWorldRect(terrainPosition.x, terrainPosition.z, terrainData.size.x, terrainData.size.z);
            return new int2(terrainRect.width / config.ParentCellSize);
        }

        private void ShowDebugInfo() {
            Debug.Log($"FlowFieldManagerystem. Parent cells: count: {ParentFlowFieldCells.Length}. Is created: {ParentFlowFieldCells.IsCreated}. All parent cells:");
            int i = 0;
            foreach (var cell in ParentFlowFieldCells) {
                Debug.Log($"Cell {i}. World pos: {cell.WorldPosition}. Base cost: {cell.BaseCost}. Child cells capacity: {cell.ChildCells.Capacity}. \nBest cost: {cell.BestCost}. " +
                          $"Best direction: {cell.BestDirection}");
                i++;
            }
            Debug.Log($"Child cells is created: {ParentFlowFieldCells[0].ChildCells.IsCreated}");
            foreach (var cell in ParentFlowFieldCells[0].ChildCells) {
                Debug.Log($"Child Cell {i}. World pos: {cell.WorldPosition}. Base cost: {cell.BaseCost}. ");
                i++;
            }
        }
        
        private readonly struct CreateChildNativeListsJob : IJob {
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
            private readonly float _childCellSize;

            public CreateChildNativeListsJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, float childCellSize) {
                _parentCellsWriter = parentCellsWriter;
                _childCellSize = childCellSize;
            }
            
            public unsafe void Execute() {
                var anyParentCell = _parentCellsWriter.ListData->Ptr[0];
                var gridSize = new int2(Mathf.FloorToInt(anyParentCell.WorldRect.Width / _childCellSize),
                    Mathf.FloorToInt(anyParentCell.WorldRect.Height / _childCellSize));
                
                for (int i = 0; i < _parentCellsWriter.ListData->Length; i++) {
                    var cell = _parentCellsWriter.ListData->Ptr[i];
                    cell.ChildCells = new UnsafeList<FlowfieldCellComponent>(gridSize.x * gridSize.y, Allocator.Persistent);
                    cell.ChildGridSize = gridSize;
                    _parentCellsWriter.ListData->Ptr[i] = cell;
                }
            }
        }
    }
}