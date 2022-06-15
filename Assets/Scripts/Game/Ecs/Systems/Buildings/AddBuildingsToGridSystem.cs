using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Systems {
    public partial class AddBuildingsToGridSystem : SystemBase {
        private GridKeeperSystem _gridKeeperSystem;
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;
        
        protected override void OnCreate() {
            _gridKeeperSystem = World.GetOrCreateSystem<GridKeeperSystem>();
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            Entities.WithAll<Tag_ReadyForGridQuad>().ForEach((DynamicBuffer<Int2BufferElement> positionsBuffer, ref Entity entity) => {
                var ecb = _commandBufferSystem.CreateCommandBuffer();
                var positionValues = new NativeArray<int2>(positionsBuffer.Length, Allocator.Temp);
                for (int i = 0; i < positionsBuffer.Length; i++)
                    positionValues[i] = positionsBuffer[i].value;
                _gridKeeperSystem.BuildingGrid.AddBuildingToGrid(positionValues, entity);
                positionValues.Dispose();
                ecb.DestroyEntity(entity);
            }).WithoutBurst().Run();
        }
    }
}