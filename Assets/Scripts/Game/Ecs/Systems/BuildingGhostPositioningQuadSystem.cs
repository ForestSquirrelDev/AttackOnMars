using Game.Ecs.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Ecs.Systems {
    public class BuildingGhostPositioningQuadSystem : SystemBase {
        private GridKeeperSystem _gridKeeper;
        private EntityQueryDesc _quadsQueryDescription;
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

        protected override void OnCreate() {
            _gridKeeper = World.GetOrCreateSystem<GridKeeperSystem>();
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _quadsQueryDescription = new EntityQueryDesc {
                All = new ComponentType[] { typeof(BuildingGhostPositioningQuadComponent), typeof(LocalToWorld)},
            };
        }

        protected override void OnUpdate() {
            
        }

        private struct UpdatePositionsJob : IJobChunk {
            [ReadOnly] public PositioningGrid PositioningGrid;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                throw new System.NotImplementedException();
            }
        }
    }
}