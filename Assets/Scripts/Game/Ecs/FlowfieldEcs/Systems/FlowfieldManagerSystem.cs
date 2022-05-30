using System.Threading.Tasks;
using Game.Ecs.Flowfield.Components;
using Game.Ecs.Flowfield.Configs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;

namespace Game.Ecs.Flowfield.Systems {
    public partial class FlowfieldManagerSystem : SystemBase {
        public bool Initialized { get; private set; }
        public NativeArray<FlowfieldCellComponent> ParentFlowFieldCells;
        private FlowfieldJobDependenciesHandler _jobDependenciesHandler;

        private FlowfieldConfig _flowfieldConfig;
        private TerrainData _terrainData;

        protected override void OnCreate() {
            _jobDependenciesHandler = new FlowfieldJobDependenciesHandler();
            _jobDependenciesHandler.OnCreate();
        }

        public async void Awake(Transform terrainTransform) {
            _flowfieldConfig = await LoadFlowfieldConfig();
            _terrainData = await LoadTerrainData();
            var terrainPosition = terrainTransform.position;
            var gridSize = FindParentGridSize(terrainPosition, _terrainData, _flowfieldConfig);
            ParentFlowFieldCells = new NativeArray<FlowfieldCellComponent>(gridSize.x * gridSize.y, Allocator.Persistent);
            var fillParentCellsJob = new FillEmptyCellsJob {
                CellSize = _flowfieldConfig.ParentCellSize, GridSize = gridSize, Origin = terrainPosition, FlowFieldCells = ParentFlowFieldCells
            };
            Schedule(fillParentCellsJob, 4);
            Initialized = true;
        }
        
        protected override void OnUpdate() {
            _jobDependenciesHandler.OnUpdate();
        }

        protected override void OnDestroy() {
            _jobDependenciesHandler.OnDestroy();
            ParentFlowFieldCells.Dispose();
        }
        
        public JobHandle Schedule<T>(T flowfieldRelatedJob, int framesLifetime = 4) where T: struct, IJob {
            return _jobDependenciesHandler.ScheduleReadWrite(flowfieldRelatedJob, framesLifetime);
        }

        public void CompleteAllFlowfieldJobs() {
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