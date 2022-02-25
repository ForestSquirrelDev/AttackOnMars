using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems {
    public class AddBuildingsToGridSystem : SystemBase {
        private GridKeeperSystem _gridKeeperSystem;
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;
        
        protected override void OnCreate() {
            _gridKeeperSystem = World.GetOrCreateSystem<GridKeeperSystem>();
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            Entities.WithAll<Tag_ReadyForGridQuad>().ForEach((DynamicBuffer<Int2BufferElement> positions, ref Entity entity) => {
                var ecb = _commandBufferSystem.CreateCommandBuffer();
                _gridKeeperSystem.buildingGrid.AddBuildingToGrid(positions, entity);
                ecb.DestroyEntity(entity);
            }).WithoutBurst().Run();
        }
    }
}