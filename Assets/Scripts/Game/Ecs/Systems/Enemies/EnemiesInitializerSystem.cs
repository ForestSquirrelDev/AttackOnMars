using Game.Ecs.Components;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Ecs.Systems.Spawners {
    public partial class EnemiesInitializerSystem : SystemBase {
        private EndSimulationEntityCommandBufferSystem _ecb;
        
        protected override void OnCreate() {
            _ecb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var ecb = _ecb.CreateCommandBuffer().AsParallelWriter();
            
            Entities.WithAll<Tag_Enemy>().WithNone<UnitComponent>().WithBurst(synchronousCompilation: false).ForEach(
            (int entityInQueryIndex, ref Entity entity, in UnitInitializerComponent initializerComponent, in LocalToWorld localToWorld) => {
                ecb.AddComponent<UnitComponent>(entityInQueryIndex, entity);
                ecb.AddBuffer<UnitBufferElement>(entityInQueryIndex, entity);
                var unitComponent = CreateUnitComponent(localToWorld, initializerComponent);
                ecb.SetComponent(entityInQueryIndex, entity, unitComponent);
            }).ScheduleParallel();
            
            _ecb.AddJobHandleForProducer(Dependency);
        }
        
        private static UnitComponent CreateUnitComponent(LocalToWorld localToWorld, UnitInitializerComponent initializerComponent) {
            float3 currentPosition = localToWorld.Position;
            float3 targetPosition = new float3(currentPosition.x, currentPosition.y, currentPosition.z + initializerComponent.DestinationDistanceAlongZAxis);
            UnitComponent unitComponent = new UnitComponent();
            unitComponent.FromPosition = localToWorld.Position;
            unitComponent.ToPosition = targetPosition;
            unitComponent.CurrentBufferIndex = 0;
            unitComponent.Speed = new Random(initializerComponent.SpeedSeed)
                .NextFloat(initializerComponent.SpeedRange.x, initializerComponent.SpeedRange.y);
            unitComponent.ReachedDistanceThreshold = initializerComponent.ReachedDistanceThreshold;
            return unitComponent;
        }
    }
}
