using System.Threading.Tasks;
using Game.Ecs.Flowfield.Components;
using Game.Ecs.Flowfield.Configs;
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
        private EmptyCellsGenerationSystem _emptyCellsGenerationSystem;

        private FlowfieldConfig _flowfieldConfig;
        private TerrainData _terrainData;

        protected override void OnCreate() {
            _jobDependenciesHandler = new FlowfieldJobDependenciesHandler();
            _emptyCellsGenerationSystem = new EmptyCellsGenerationSystem(_jobDependenciesHandler);
            _jobDependenciesHandler.OnCreate();
            _emptyCellsGenerationSystem.OnCreate();
        }

        public async void Awake(Transform terrainTransform) {
            _flowfieldConfig = await LoadFlowfieldConfig();
            _terrainData = await LoadTerrainData();
            var terrainPosition = terrainTransform.position;
            var parentGridSize = FindParentGridSize(terrainPosition, _terrainData, _flowfieldConfig);
            
            ParentFlowFieldCells = new UnsafeList<FlowfieldCellComponent>(parentGridSize.x * parentGridSize.y, Allocator.Persistent);
            ChildCells = new NativeList<UnsafeList<FlowfieldCellComponent>>(ParentFlowFieldCells.Length, Allocator.Persistent);
            
            _emptyCellsGenerationSystem.EnqueueCellsGenerationRequest(
                new EmptyCellsGenerationSystem.EmptyCellsGenerationRequest(_flowfieldConfig.ParentCellSize, terrainPosition, parentGridSize, 
                    ParentFlowFieldCells.AsParallelWriter()));

            Initialized = true;
        }

        protected override void OnUpdate() {
            _jobDependenciesHandler.OnUpdate();
            _emptyCellsGenerationSystem.OnUpdate();
            if (Input.GetKeyDown(KeyCode.I)) {
                Debug.Log($"{ParentFlowFieldCells.Length}. Is created: {ParentFlowFieldCells.IsCreated}");
            }
        }

        protected override void OnDestroy() {
            _jobDependenciesHandler.OnDestroy();
            _emptyCellsGenerationSystem.OnDestroy();
            ParentFlowFieldCells.Clear();
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
    }
}