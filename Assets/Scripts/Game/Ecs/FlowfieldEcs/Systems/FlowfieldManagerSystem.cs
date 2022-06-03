using System.Threading.Tasks;
using Experiments;
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
    // Flowfield level 0.5. Initialize grid systems and schedule creation of empty parent grid.
    public partial class FlowfieldManagerSystem : SystemBase {
        public bool Initialized { get; private set; }
        
        public UnsafeList<FlowfieldCellComponent> ParentFlowFieldCells;
        public NativeList<UnsafeList<FlowfieldCellComponent>> ChildCells;
        
        private FlowfieldJobDependenciesHandler _jobDependenciesHandler;
        private EmptyCellsGenerationSubSystem _emptyCellsGenerationSubSystem;
        private FindBaseCostAndHeightsSubSystem _findBaseCostAndHeightsSubSystem;

        private FlowfieldConfig _flowfieldConfig;
        private TerrainData _terrainData;

        protected override void OnCreate() {
            _jobDependenciesHandler = new FlowfieldJobDependenciesHandler();
            _findBaseCostAndHeightsSubSystem = new FindBaseCostAndHeightsSubSystem(_jobDependenciesHandler);
            _emptyCellsGenerationSubSystem = new EmptyCellsGenerationSubSystem(_jobDependenciesHandler, _findBaseCostAndHeightsSubSystem);
            
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
                Debug.Log($"Cell {i}. World pos: {cell.WorldPosition}. Base cost: {cell.BaseCost}");
                i++;
            }
        }
    }
}