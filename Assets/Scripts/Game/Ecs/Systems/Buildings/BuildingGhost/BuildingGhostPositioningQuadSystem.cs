using Game.Ecs.Components;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems {
    public partial class BuildingGhostPositioningQuadSystem : SystemBase {
        private GridKeeperSystem _gridKeeper;

        protected override void OnCreate() {
            _gridKeeper = World.GetOrCreateSystem<GridKeeperSystem>();
        }

        protected override void OnUpdate() {
            var xzMin = new float4(-0.5f, 0f, -0.5f, 1);
            var xzMax = new float4(0.5f, 0f, 0.5f, 1);
            var buildingGrid = _gridKeeper.BuildingGrid;

            var dependencies = JobHandle.CombineDependencies(_gridKeeper.DependenciesScheduler.GetCombinedReadWriteDependencies(), Dependency);
            
            Dependency = Entities.WithAll<BuildingGhostPositioningQuadComponent>().ForEach((ref BuildingGhostPositioningQuadComponent quadComponent, in LocalToWorld ltw) => {
                var ltwMatrix = ltw.Value;
                (ltwMatrix[1], ltwMatrix[2]) = (ltwMatrix[2], ltwMatrix[1]);

                float4 xzMinWorld = math.mul(ltwMatrix, xzMin);
                float4 xzMaxWorld = math.mul(ltwMatrix, xzMax);

                Rect rect = new Rect {
                    xMin = xzMinWorld.x,
                    yMin = xzMinWorld.z,
                    xMax = xzMaxWorld.x,
                    yMax = xzMaxWorld.z
                };
                
                quadComponent.AvailableForPlacement = !buildingGrid.IntersectsWithOccupiedTiles(rect);
            }).Schedule(dependencies);
            
            _gridKeeper.DependenciesScheduler.AddExternalReadOnlyDependency(Dependency);
        }
    }
}