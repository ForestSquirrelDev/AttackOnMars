using System.Collections.Generic;
using Game.Ecs.Components;
using Game.Ecs.Components.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;
using Utils.ThirdParty;

namespace Game.Ecs.Systems.Spawners {
    public partial class UnitSystem : SystemBase {
        private Dictionary<int, float3[]> allPaths = new Dictionary<int, float3[]>();
        private List<Entity> _routedEntities = new List<Entity>();
        private List<NativeArray<int>> _statusOutputs = new List<NativeArray<int>>();
        private List<NativeList<float3>> _resultingPaths = new List<NativeList<float3>>();
        private List<NavMeshQuery> _navMeshQueries = new List<NavMeshQuery>();
        private List<JobHandle> _jobHandles = new List<JobHandle>();

        private NavMeshWorld _navMeshWorld;
        private NavMeshQuery _navMeshQuery;
        private EnemiesSingletonData _enemiesSingletonData;
        private float3 _maxSearchDistance = new float3(10, 10, 10);
        private int _maxNodesTraversePerUpdate;

        protected override void OnCreate() {
            for (int i = 0; i < _enemiesSingletonData.MaxEntitiesRoutedPerFrame; i++) {
                _resultingPaths.Add(new NativeList<float3>(1024, Allocator.Persistent));
                _statusOutputs.Add(new NativeArray<int>(3, Allocator.Persistent));
            }
            _navMeshWorld = NavMeshWorld.GetDefaultWorld();
        }

        protected override void OnStartRunning() {
            _enemiesSingletonData = GetSingleton<EnemiesSingletonData>();
        }

        protected override void OnUpdate() {
            return;
            var enemiesData = GetSingleton<EnemiesSingletonData>();
            int maxNodesTraverse = _maxNodesTraversePerUpdate;
            int i = 0;
            Entities.WithNone<Tag_UnitRouted>()
            .ForEach((Entity entity, ref UnitComponent unitComponent, ref DynamicBuffer<UnitBufferElement> UnitBuffer) => {
                if (!unitComponent.Routed) {
                    var currentQuery = new NavMeshQuery(_navMeshWorld, Allocator.TempJob, enemiesData.MaxPathNodePoolSize);
                    var pathfindingJob = ConstructPathfindingJob(ref currentQuery, unitComponent, maxNodesTraverse, i, ref UnitBuffer);
                }
            }).WithoutBurst().WithStructuralChanges().Run();
        }
        
        private SinglePathfindingJob ConstructPathfindingJob(ref NavMeshQuery currentQuery, UnitComponent unitComponent, int maxNodesTraverse, int i, ref DynamicBuffer<UnitBufferElement> UnitBuffer) {
            return new SinglePathfindingJob {
                NavMeshQuery = currentQuery,
                FromLocation = unitComponent.FromLocation,
                ToLocation = unitComponent.ToLocation,
                FromPosition = unitComponent.FromPosition,
                ToPosition = unitComponent.ToPosition,
                MaxNodesTraversePerUpdate = maxNodesTraverse,
                ResultingPath = _resultingPaths[i],
                StatusOutput = _statusOutputs[i],
                MaxPathSize = _enemiesSingletonData.MaxPathSize,
                UnitBuffer = UnitBuffer,
                AgentTypeID = 0,
                MaxSearchDistance = float.MaxValue
            };
        }

        protected override void OnDestroy() {
            for (int i = 0; i < _enemiesSingletonData.MaxEntitiesRoutedPerFrame; i++) {
                _resultingPaths[i].Dispose();
                _statusOutputs[i].Dispose();
            }
        }

        [BurstCompile]
        private struct SinglePathfindingJob : IJob {
            private PathQueryStatus _navMeshQueryStatus;
            private PathQueryStatus _pathUtilsQueryStatus;

            public NavMeshQuery NavMeshQuery;
            public NavMeshLocation FromLocation;
            public NavMeshLocation ToLocation;
            public float3 FromPosition;
            public float3 ToPosition;
            public float3 MaxSearchDistance;
            public int MaxNodesTraversePerUpdate;
            public DynamicBuffer<UnitBufferElement> UnitBuffer;
            public NativeList<float3> ResultingPath;
            public NativeArray<int> StatusOutput;
            public int MaxPathSize;
            public int AgentTypeID;

            private float3 _elevatedPoint;

            public void Execute() {
                FromLocation = NavMeshQuery.MapLocation(FromPosition, MaxSearchDistance, AgentTypeID);
                ToLocation = NavMeshQuery.MapLocation(ToPosition, MaxSearchDistance, AgentTypeID);
                if (!NavMeshQuery.IsValid(FromLocation) || !NavMeshQuery.IsValid(ToLocation)) return;
                
                _navMeshQueryStatus = NavMeshQuery.BeginFindPath(FromLocation, ToLocation);
                switch (_navMeshQueryStatus) {
                    case PathQueryStatus.InProgress:
                        NavMeshQuery.UpdateFindPath(MaxNodesTraversePerUpdate, out _);
                        break;
                    case PathQueryStatus.Success: {
                        _navMeshQueryStatus = NavMeshQuery.EndFindPath(out int pathSize);
                        
                        var resultPath = new NativeArray<NavMeshLocation>(pathSize, Allocator.Temp);
                        var straightPathFlags = new NativeArray<StraightPathFlags>();
                        var vertexSide = new NativeArray<float>(MaxPathSize, Allocator.Temp);
                        var polygonIds = new NativeArray<PolygonId>(pathSize, Allocator.Temp);
                        int straightPathCount = 0;
                        
                        NavMeshQuery.GetPathResult(polygonIds);
                        _pathUtilsQueryStatus = PathUtils.FindStraightPath(NavMeshQuery, FromPosition, ToPosition,
                            polygonIds, pathSize, ref resultPath, ref straightPathFlags, 
                            ref vertexSide, ref straightPathCount, MaxPathSize);
                        
                        if (_pathUtilsQueryStatus == PathQueryStatus.Success) {
                            FillSuccessJobOutputs(straightPathCount, resultPath);
                        }
                        
                        resultPath.Dispose();
                        straightPathFlags.Dispose();
                        vertexSide.Dispose();
                        polygonIds.Dispose();
                        break;
                    }
                }
            }
            
            private void FillSuccessJobOutputs(int straightPathCount, NativeArray<NavMeshLocation> resultPath) {
                int fromKey = ((int)FromPosition.x + (int)FromPosition.y + (int)FromPosition.z) * MaxPathSize;
                int toKey = ((int)ToPosition.x + (int)ToPosition.y + (int)ToPosition.z) * MaxPathSize;
                int resultKey = fromKey + toKey;
                StatusOutput[0] = 1;
                StatusOutput[1] = resultKey;
                StatusOutput[2] = straightPathCount;

                for (int i = 0; i < straightPathCount; i++) {
                    ResultingPath[i] = resultPath[i].position;
                    UnitBuffer.Add(new UnitBufferElement { Waypoint = ResultingPath[i] });
                }
            }
        }
    }
}