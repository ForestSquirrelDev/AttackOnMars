using System;
using System.Collections.Generic;
using Game.AddressableConfigs;
using Game.Ecs.Components;
using Game.Ecs.Components.BlobAssetsData;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Game.Ecs.Systems.Pathfinding;
using Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Spawners {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class SpawnEnemiesSystem : SystemBase {
        private int _counter;
        private int _sortKey;
        private int _enemiesEnumCount;
        private ConvertedEnemiesBlobAssetReference _enemiesReference;
        private EndSimulationEntityCommandBufferSystem _ecb;
        private EnemiesSpawnerConfig _config;
        private Terrain _terrain;
        private List<SpawnPointData> _spawnPoints;

        private bool _isRunning;
        private bool _inited;

        private float _countPerFrame;

        protected override void OnCreate() {
            RequireSingletonForUpdate<MainHumanBaseSingletonComponent>();
            _config = AddressablesLoader.Get<EnemiesSpawnerConfig>(AddressablesConsts.DefaultEnemiesSpawnerConfig);
            _countPerFrame = _config.CountPerFrame;
            _enemiesEnumCount = Enum.GetNames(typeof(EnemyType)).Length;
            _ecb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        public void Init(Terrain terrain) {
            _terrain = terrain;
            _inited = true;
        }

        protected override void OnStartRunning() {
            if (_isRunning) return;
            
            _enemiesReference = GetSingleton<ConvertedEnemiesBlobAssetReference>();
            _spawnPoints = new List<SpawnPointData>();
            Entities.WithAll<EnemySpawnPoint>().ForEach((in LocalToWorld ltw, in EnemySpawnPoint spawnPoint) => {
                _spawnPoints.Add(new SpawnPointData(ltw.Position, spawnPoint.SpawnRadius));
            }).WithoutBurst().Run();
            
            _isRunning = true;
        }

        protected override void OnUpdate() {
            if (!_inited) return;
            if (Input.GetKeyDown(KeyCode.F1)) {
                _counter = 0;
                Entities.WithAll<Tag_Enemy>().ForEach((ref Entity e) => {
                    EntityManager.DestroyEntity(e);
                }).WithStructuralChanges().WithoutBurst().Run();
            }
            if (_counter >= _config.EnemiesCount) return;
            _sortKey++;

            var spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)];
            if (_countPerFrame < 1) {
                _countPerFrame += _countPerFrame;
                return;
            }
            for (int i = 0; i < _countPerFrame; i++) {
                float3 translation = (float3)UnityEngine.Random.insideUnitSphere * spawnPoint.Radius + spawnPoint.WorldPos;
                var y = _terrain.SampleHeight(translation);
                var ecb = _ecb.CreateCommandBuffer().AsParallelWriter();
                var spawnEnemiesJob = new SpawnEnemyJob {
                    EnemiesReference = _enemiesReference,
                    Ecb = ecb,
                    EnemyType = (EnemyType)UnityEngine.Random.Range(0, _enemiesEnumCount),
                    SortKey = _sortKey, 
                    InitialPos = new float3(translation.x, y, translation.z)
                };
            
                var handle = spawnEnemiesJob.Schedule();
                _ecb.AddJobHandleForProducer(handle);
                Dependency = handle;
                _counter++;
            }

            _countPerFrame = _config.CountPerFrame;
        }

        public void OnDrawGizmos() {
            foreach (var spawnPoint in _spawnPoints) {
                Gizmos.DrawWireSphere(spawnPoint.WorldPos, spawnPoint.Radius);
            }
        }

        private struct SpawnPointData {
            public float3 WorldPos;
            public float Radius;
            
            public SpawnPointData(float3 worldPos, float radius) {
                WorldPos = worldPos;
                Radius = radius;
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