using System;
using Game.Ecs.Components.BlobAssetsData;
using Game.Ecs.Components.Tags;
using Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Game.Ecs.Systems.Spawners {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class SpawnEnemiesSystem : SystemBase {
        private int _counter;
        private int _sortKey;
        private int _enemiesEnumCount;
        private ConvertedEnemiesBlobAssetReference _reference;
        private EndSimulationEntityCommandBufferSystem _ecb;
        private Random _positionRandomizer;
        private Random _enemyTypeRandomizer;

        protected override void OnCreate() {
            _enemiesEnumCount = Enum.GetNames(typeof(EnemyType)).Length;
        }

        protected override void OnStartRunning() {
            _reference = GetSingleton<ConvertedEnemiesBlobAssetReference>();
            _ecb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _positionRandomizer = new Random(1);
            _enemyTypeRandomizer = new Random(1);
        }

        protected override void OnUpdate() {
            if (Input.GetKeyDown(KeyCode.F1)) {
                _counter = 0;
                Entities.WithAll<Tag_Enemy>().ForEach((ref Entity e) => {
                    EntityManager.DestroyEntity(e);
                }).WithStructuralChanges().WithoutBurst().Run();
            }
            if (_counter > 500) return;
            _sortKey++;

            for (int i = 0; i < 1; i++) {
                float3 translation = new float3 {
                    x = _positionRandomizer.NextFloat(0f, 1665f),
                    y = 90,
                    z = _positionRandomizer.NextFloat(0f, 1800f)
                };
                var ecb = _ecb.CreateCommandBuffer().AsParallelWriter();
                var spawnEnemiesJob = new SpawnEnemyJob {
                    EnemiesReference = _reference,
                    Ecb = ecb,
                    EnemyType = (EnemyType)_enemyTypeRandomizer.NextInt(0, _enemiesEnumCount),
                    SortKey = _sortKey, 
                    InitialPos = translation
                };
            
                var handle = spawnEnemiesJob.Schedule();
                _ecb.AddJobHandleForProducer(handle);
                Dependency = handle;
                _counter++;
            }
        }
        
        [BurstCompile]
        private struct SpawnEnemyJob : IJob {
            [ReadOnly] public EnemyType EnemyType;
            [ReadOnly] public int SortKey;
            [ReadOnly] public float3 InitialPos;
            public ConvertedEnemiesBlobAssetReference EnemiesReference;
            public EntityCommandBuffer.ParallelWriter Ecb;

            public void Execute() {
                ref var enemiesArray = ref EnemiesReference.EnemiesReference.Value.EnemiesArray;
                var entity = Ecb.Instantiate(SortKey, FindEntitySlow(ref enemiesArray, EnemyType));
                Ecb.SetComponent(SortKey, entity, new Translation{Value = InitialPos});
            }

            private Entity FindEntitySlow(ref BlobArray<ConvertedEnemyBlobData> enemies, EnemyType enemyType) {
                for (int i = 0; i < enemies.Length; i++) {
                    var enemy = enemies[i];
                    if (enemy.EnemyType == enemyType) return enemy.EnemyEntity;
                }
                return Entity.Null;
            }
        }
    }
}