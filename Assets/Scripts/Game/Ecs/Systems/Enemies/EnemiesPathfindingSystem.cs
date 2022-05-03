using Game.Ecs.Components.BufferElements;
using Game.Ecs.Components.Tags;
using Unity.AI.Navigation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;
using Utils;
using Utils.ThirdParty;

namespace Game.Ecs.Systems.Enemies {
    public partial class EnemiesPathfindingSystem : SystemBase {
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

        protected override void OnCreate() {
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var entityTypeHandle = _commandBufferSystem.GetEntityTypeHandle();
            var ecb = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        }
    }
}