using System.Threading.Tasks;
using Game.AddressableConfigs;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Systems.Spawners;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;

namespace Game.Ecs.Systems.Pathfinding {
    // Flowfield level 0.5. Initialize grid systems and schedule creation of empty parent grid.
    public partial class FlowfieldManagerSystem : SystemBase {
        public bool Initialized { get; private set; }
        
        public UnsafeList<FlowfieldCellComponent> ParentFlowFieldCells;

        private FlowfieldJobDependenciesHandler _jobDependenciesHandler;
        private EmptyCellsGenerationSubSystem _emptyCellsGenerationSubSystem;
        private FindBaseCostAndHeightsSubSystem _findBaseCostAndHeightsSubSystem;
        private GenerateIntegrationFieldSubsystem _generateIntegrationFieldSubsystem;
        private GenerateFlowFieldSubsystem _generateFlowFieldSubsystem;
        private DetectEnemiesAndScheduleChildCellsSystem _detectEnemiesSystem;
        private ManageChildCellsGenerationRequestsSystem _childCellsGenerationSubsystem;

        private FlowfieldConfig _flowfieldConfig;
        private FlowfieldRuntimeData _flowfieldRuntimeData;
        private TerrainData _terrainData;
        
        public async Task Awake() {
            _flowfieldConfig = await LoadFlowfieldConfig();
            _terrainData = await LoadTerrainData();
            GetSubsystems();
            CallSubsystemsOnCreate();
        }
        
        private void GetSubsystems() {
            _jobDependenciesHandler = new FlowfieldJobDependenciesHandler();
            _findBaseCostAndHeightsSubSystem = new FindBaseCostAndHeightsSubSystem(_jobDependenciesHandler);
            _emptyCellsGenerationSubSystem = new EmptyCellsGenerationSubSystem(_jobDependenciesHandler, _findBaseCostAndHeightsSubSystem);
            _generateIntegrationFieldSubsystem = new GenerateIntegrationFieldSubsystem(_jobDependenciesHandler);
            _generateFlowFieldSubsystem = new GenerateFlowFieldSubsystem(_jobDependenciesHandler);
            _childCellsGenerationSubsystem = new ManageChildCellsGenerationRequestsSystem(_jobDependenciesHandler, ParentFlowFieldCells.AsParallelWriter(), _emptyCellsGenerationSubSystem,
                _findBaseCostAndHeightsSubSystem, _generateIntegrationFieldSubsystem, _generateFlowFieldSubsystem);
            _detectEnemiesSystem = World.GetOrCreateSystem<DetectEnemiesAndScheduleChildCellsSystem>();
        }
        
        private void CallSubsystemsOnCreate() {
            _jobDependenciesHandler.OnCreate();
        }

        public void Init(Transform terrainTransform) {
            _flowfieldRuntimeData = CreateFlowfieldRuntimeData(terrainTransform);
            ParentFlowFieldCells = new UnsafeList<FlowfieldCellComponent>(_flowfieldRuntimeData.ParentGridSize.x * _flowfieldRuntimeData.ParentGridSize.y, Allocator.Persistent);
            InjectSubsystemsDependencies();
            InitializeParentGrid();
            Initialized = true;
        }
        
        private FlowfieldRuntimeData CreateFlowfieldRuntimeData(Transform terrainTransform) {
            var terrainPosition = terrainTransform.position;
            var parentGridSize = FindParentGridSize(terrainPosition, _terrainData, _flowfieldConfig);
            var childGridSize = new int2(Mathf.FloorToInt(_flowfieldConfig.ParentCellSize / _flowfieldConfig.ChildCellSize),
                Mathf.FloorToInt(_flowfieldConfig.ParentCellSize / _flowfieldConfig.ChildCellSize));
            var runtimeData = new FlowfieldRuntimeData(terrainPosition, parentGridSize, childGridSize, _flowfieldConfig.ParentCellSize, _flowfieldConfig.ChildCellSize,
                _flowfieldConfig.UnwalkableAngleThreshold, _flowfieldConfig.CostHeightThreshold);
            return runtimeData;
        }
        
        private void InjectSubsystemsDependencies() {
            _detectEnemiesSystem.Construct(_jobDependenciesHandler, _flowfieldRuntimeData, _childCellsGenerationSubsystem);
            _childCellsGenerationSubsystem.Construct(_flowfieldRuntimeData.ParentGridSize, _flowfieldRuntimeData);
        }
        
        private void InitializeParentGrid() {
            var fillEmptyCellsJob = _emptyCellsGenerationSubSystem.Schedule(_flowfieldConfig.ParentCellSize, _flowfieldRuntimeData.ParentGridSize,
                _flowfieldRuntimeData.ParentGridOrigin, ParentFlowFieldCells.AsParallelWriter(), default(JobHandle));
            var fillHeightsJob = _findBaseCostAndHeightsSubSystem.Schedule(ParentFlowFieldCells.AsParallelWriter(), _flowfieldRuntimeData.ParentGridSize, fillEmptyCellsJob,
                _flowfieldConfig.UnwalkableAngleThreshold, _flowfieldConfig.CostHeightThreshold);
            var createChildListsJob = _jobDependenciesHandler.ScheduleReadWrite(new CreateChildNativeListsJob(ParentFlowFieldCells.AsParallelWriter(), _flowfieldConfig.ChildCellSize), dependenciesIn: fillHeightsJob);
        }
        
        protected override void OnUpdate() {
            if (!Initialized) return;
            _jobDependenciesHandler.OnUpdate();
            _childCellsGenerationSubsystem.OnUpdate();

            if (Input.GetKeyDown(KeyCode.I)) {
                ShowDebugInfo();
            }
        }

        protected override void OnDestroy() {
            _jobDependenciesHandler.OnDestroy();
            _childCellsGenerationSubsystem.OnDestroy();
            foreach (var cell in ParentFlowFieldCells) {
                cell.Dispose();
            }
            ParentFlowFieldCells.Dispose();
        }
        
        public JobHandle ScheduleReadWrite<T>(T flowfieldRelatedJob, int framesLifetime = 4) where T: struct, IJob {
            return _jobDependenciesHandler.ScheduleReadWrite(flowfieldRelatedJob, framesLifetime);
        }
        
        public JobHandle ScheduleReadOnly<T>(T flowfieldRelatedJob, int framesLifetime = 1) where T: struct, IJob {
            return _jobDependenciesHandler.ScheduleReadOnly(flowfieldRelatedJob, framesLifetime);
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
                Debug.Log($"Cell {i}. World pos: {cell.WorldPosition}. Base cost: {cell.BaseCost}. Child cells capacity: {cell.ChildCells.Capacity}." +
                          $"\nBest cost: {cell.BestCost}. " +
                          $"Best direction: {cell.BestDirection}");
                i++;
            }
            Debug.Log($"Child cells is created: {ParentFlowFieldCells[0].ChildCells.IsCreated}");
            for (var index = 0; index < ParentFlowFieldCells.Length; index++) {
                var cell = ParentFlowFieldCells[index];
                var childCells = cell.ChildCells;
                for (int j = 0; j < childCells.Length; j++) {
                    var childCell = childCells[j];
                    Debug.Log($"Child Cell {j}. World pos: {childCell.WorldPosition}. Base cost: {childCell.BaseCost}. ");
                }
            }
        }
        
        [BurstCompile]
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