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
using Utils;

namespace Game.Ecs.Systems.Pathfinding {
    public partial class FlowfieldManagerSystem : SystemBase {
        public bool Initialized { get; private set; }
        
        // it's public strictly for debug reasons (FlowFieldGizmosDrawer.cs)
        public UnsafeList<FlowfieldCellComponent> ParentFlowFieldCells;

        // systems/subsystems that are either child objects fo flowfieldmanager or tightly coupled with its dependencies
        private DependenciesScheduler _jobDependenciesScheduler;
        private EmptyCellsGenerationSubSystem _emptyCellsGenerationSubSystem;
        private FindBaseCostAndHeightsSubSystem _findBaseCostAndHeightsSubSystem;
        private GenerateIntegrationFieldSubsystem _generateIntegrationFieldSubsystem;
        private GenerateFlowFieldSubsystem _generateFlowFieldSubsystem;
        private DetectEnemiesAndScheduleChildCellsSystem _detectEnemiesSystem;
        private ManageChildCellsGenerationRequestsSystem _childCellsGenerationSystem;
        private AssignBestGridDirectionToEnemiesSystem _assignBestGridDirectionToEnemiesSystem;
        private SetEnemyReadyToAttackStateSystem _setEnemyReadyToAttackStateSystem;

        private FlowfieldConfig _flowfieldConfig;
        private TerrainData _terrainData;
        private FlowfieldRuntimeData _flowfieldRuntimeData;

        protected override void OnCreate() {
            _flowfieldConfig = AddressablesLoader.Get<FlowfieldConfig>(AddressablesConsts.FlowfieldConfig);
            _terrainData = AddressablesLoader.Get<TerrainData>(AddressablesConsts.DefaultTerrainData);
        }

        public void Init(Transform terrainTransform) {
            GetSubsystems();
            InitSelfGlobalDependencies(terrainTransform);
            InjectSubsystemsDependencies();
            InitializeParentGrid();
            Initialized = true;
        }

        private void GetSubsystems() {
            _jobDependenciesScheduler = new DependenciesScheduler();
            _findBaseCostAndHeightsSubSystem = new FindBaseCostAndHeightsSubSystem(_jobDependenciesScheduler);
            _emptyCellsGenerationSubSystem = new EmptyCellsGenerationSubSystem(_jobDependenciesScheduler);
            _generateIntegrationFieldSubsystem = new GenerateIntegrationFieldSubsystem(_jobDependenciesScheduler);
            _generateFlowFieldSubsystem = new GenerateFlowFieldSubsystem(_jobDependenciesScheduler);
            _childCellsGenerationSystem = World.GetOrCreateSystem<ManageChildCellsGenerationRequestsSystem>();
            _detectEnemiesSystem = World.GetOrCreateSystem<DetectEnemiesAndScheduleChildCellsSystem>();
            _assignBestGridDirectionToEnemiesSystem = World.GetOrCreateSystem<AssignBestGridDirectionToEnemiesSystem>();
            _setEnemyReadyToAttackStateSystem = World.GetOrCreateSystem<SetEnemyReadyToAttackStateSystem>();
        }

        private void InitSelfGlobalDependencies(Transform terrainTransform) {
            _flowfieldRuntimeData = CreateFlowfieldRuntimeData(terrainTransform);
            ParentFlowFieldCells = new UnsafeList<FlowfieldCellComponent>(_flowfieldRuntimeData.ParentGridSize.x * _flowfieldRuntimeData.ParentGridSize.y, Allocator.Persistent);
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
            _detectEnemiesSystem.InjectFlowfieldDependencies(_jobDependenciesScheduler, _flowfieldRuntimeData, _childCellsGenerationSystem);
            _childCellsGenerationSystem.InjectFlowfieldDependencies(_jobDependenciesScheduler, ParentFlowFieldCells.AsParallelWriter(), _emptyCellsGenerationSubSystem,
                _findBaseCostAndHeightsSubSystem, _generateIntegrationFieldSubsystem, _generateFlowFieldSubsystem, _flowfieldRuntimeData.ParentGridSize, _flowfieldRuntimeData);
            _assignBestGridDirectionToEnemiesSystem.InjectFlowfieldDependencies(_jobDependenciesScheduler, ParentFlowFieldCells.AsParallelWriter(), _flowfieldRuntimeData);
            _setEnemyReadyToAttackStateSystem.InjectFlowfieldDependencies(_jobDependenciesScheduler, ParentFlowFieldCells.AsParallelWriter(), _flowfieldRuntimeData);
        }
        
        private void InitializeParentGrid() {
            var currentTarget = GetSingleton<CurrentHivemindTargetSingleton>().Value;
            var fillEmptyCellsJob = _emptyCellsGenerationSubSystem.ScheduleReadWrite(_flowfieldConfig.ParentCellSize, _flowfieldRuntimeData.ParentGridSize,
                _flowfieldRuntimeData.ParentGridOrigin, ParentFlowFieldCells.AsParallelWriter());
            
            var fillHeightsJob = _findBaseCostAndHeightsSubSystem.ScheduleReadWrite(ParentFlowFieldCells.AsParallelWriter(), _flowfieldRuntimeData.ParentGridSize, fillEmptyCellsJob,
                _flowfieldConfig.UnwalkableAngleThreshold, _flowfieldConfig.CostHeightThreshold);
            
            var createIntegrationFieldJob = _generateIntegrationFieldSubsystem.ScheduleReadWrite(
                _flowfieldRuntimeData.ParentGridOrigin, currentTarget, _flowfieldRuntimeData.ParentGridSize, ParentFlowFieldCells.AsParallelWriter(), fillHeightsJob);
            
            var createFlowfieldJob = _generateFlowFieldSubsystem.ScheduleReadWrite(ParentFlowFieldCells.AsParallelWriter(), _flowfieldRuntimeData.ParentGridSize, createIntegrationFieldJob);
            
            var createChildListsJob = _jobDependenciesScheduler.ScheduleReadWrite(
                new InitChildCellsUnsafeListsJob(ParentFlowFieldCells.AsParallelWriter(), _flowfieldConfig.ChildCellSize), dependenciesIn: createFlowfieldJob);
        }
        
        protected override void OnUpdate() {
            if (!Initialized) return;
            _jobDependenciesScheduler.OnUpdate();
            _childCellsGenerationSystem.OnUpdateManual();
            
            if (Input.GetKeyDown(KeyCode.I)) {
                ShowDebugInfo();
            }
        }

        protected override void OnDestroy() {
            _jobDependenciesScheduler.Dispose();
            foreach (var cell in ParentFlowFieldCells) {
                cell.Dispose();
            }
            ParentFlowFieldCells.Dispose();
        }
        
        public JobHandle ScheduleReadWrite<T>(T flowfieldRelatedJob, int framesLifetime = 4) where T: struct, IJob {
            return _jobDependenciesScheduler.ScheduleReadWrite(flowfieldRelatedJob, framesLifetime);
        }
        
        public JobHandle ScheduleReadOnly<T>(T flowfieldRelatedJob, int framesLifetime = 1) where T: struct, IJob {
            return _jobDependenciesScheduler.ScheduleReadOnly(flowfieldRelatedJob, framesLifetime);
        }

        private int2 FindParentGridSize(Vector3 terrainPosition, TerrainData terrainData, FlowfieldConfig config) {
            var terrainRect = TerrainUtility.GetWorldRect(terrainPosition.x, terrainPosition.z, terrainData.size.x, terrainData.size.z);
            return new int2(terrainRect.width / config.ParentCellSize);
        }

        private unsafe void ShowDebugInfo() {
            Debug.Log($"FlowFieldManagerystem. Parent cells: count: {ParentFlowFieldCells.Length}. Is created: {ParentFlowFieldCells.IsCreated}. All parent cells:");
            int i = 0;
            foreach (var cell in ParentFlowFieldCells) {
                Debug.Log($"Cell {i}. World pos: {cell.WorldPosition}. Base cost: {cell.BaseCost}. Child cells capacity: {cell.ChildCells.ListData->Capacity}." +
                          $"\nBest cost: {cell.BestCost}. " +
                          $"Best direction: {cell.BestDirection}");
                i++;
            }
            for (var index = 0; index < ParentFlowFieldCells.Length; index++) {
                var cell = ParentFlowFieldCells[index];
                var childCells = cell.ChildCells;
                Debug.Log($"Parent cell {index}. Child cells created: {childCells.ListData->IsCreated}. Child cells length: {childCells.ListData->Length}");
                for (int j = 0; j < childCells.ListData->Length; j++) {
                    var childCell = childCells.ListData->Ptr[j];
                    Debug.Log($"Child Cell {j}. World pos: {childCell.WorldPosition}. Grid pos: {childCell.GridPosition}. Base cost: {childCell.BaseCost}. Best direction: {childCell.BestDirection}. IS best: {childCell.IsBestCell}");
                }
            }
        }
        
        [BurstCompile]
        private readonly struct InitChildCellsUnsafeListsJob : IJob {
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
            private readonly float _childCellSize;

            public InitChildCellsUnsafeListsJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, float childCellSize) {
                _parentCellsWriter = parentCellsWriter;
                _childCellSize = childCellSize;
            }
            
            public unsafe void Execute() {
                var anyParentCell = _parentCellsWriter.ListData->Ptr[0];
                var gridSize = new int2(Mathf.FloorToInt(anyParentCell.WorldRect.Width / _childCellSize),
                    Mathf.FloorToInt(anyParentCell.WorldRect.Height / _childCellSize));
                
                for (int i = 0; i < _parentCellsWriter.ListData->Length; i++) {
                    var cell = _parentCellsWriter.ListData->Ptr[i];
                    cell.InitChildCells(gridSize.x * gridSize.y, Allocator.Persistent);
                    cell.ChildGridSize = gridSize;
                    _parentCellsWriter.ListData->Ptr[i] = cell;
                }
            }
        }
    }
}