using System.Collections.Generic;
using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
                NativeArray<int2> positionsInterpreted = new NativeArray<int2>(positions.Length, Allocator.Temp);
                for (int i = 0; i < positions.Length; i++)
                    positionsInterpreted[i] = positions[i].value;
                _gridKeeperSystem.buildingGrid.AddBuildingToGrid(positionsInterpreted, entity);
                positionsInterpreted.Dispose();
                ecb.DestroyEntity(entity);
            }).WithoutBurst().Run();
        }
    }
}