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
            _emptyCellsGenerationSubSystem.OnCreate();
            _findBaseCostAndHeightsSubSystem.OnCreate();
        }

        public async void Awake(Transform terrainTransform) {
            _flowfieldConfig = await LoadFlowfieldConfig();
            _terrainData = await LoadTerrainData();
            var terrainPosition = terrainTransform.position;
            var parentGridSize = FindParentGridSize(terrainPosition, _terrainData, _flowfieldConfig);
            
            ParentFlowFieldCells = new UnsafeList<FlowfieldCellComponent>(parentGridSize.x * parentGridSize.y, Allocator.Persistent);
            ChildCells = new NativeList<UnsafeList<FlowfieldCellComponent>>(ParentFlowFieldCells.Length, Allocator.Persistent);
            
            var fillEmptyCellsJob = _emptyCellsGenerationSubSystem.Schedule(_flowfieldConfig.ParentCellSize, parentGridSize, terrainPosition, ParentFlowFieldCells.AsParallelWriter(), default(JobHandle));
            var fillHeightsJob = _findBaseCostAndHeightsSubSystem.Schedule(ParentFlowFieldCells.AsParallelWriter(), fillEmptyCellsJob);
            var longLongDebugJob = new LongLongJob(2000).Schedule(fillHeightsJob);
            
            Initialized = true;
        }

        protected override void OnUpdate() {
            _jobDependenciesHandler.OnUpdate();
            _emptyCellsGenerationSubSystem.OnUpdate();
            _findBaseCostAndHeightsSubSystem.OnUpdate();

            if (Input.GetKeyDown(KeyCode.D)) {
                ShowDebugInfo();
            }
        }

        protected override void OnDestroy() {
            _jobDependenciesHandler.OnDestroy();
            _emptyCellsGenerationSubSystem.OnDestroy();
            _findBaseCostAndHeightsSubSystem.OnDestroy();
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
            foreach (var cell in ParentFlowFieldCells) {
                Debug.Log($"World pos: {cell.WorldPosition}");
            }
        }
    }
}