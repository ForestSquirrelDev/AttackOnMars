using Game.AddressableConfigs;
using Game.Ecs.Systems.Buildings;
using Game.Ecs.Systems.Pathfinding;
using Game.Ecs.Systems.Spawners;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Ecs.Systems {
    public class ConfigsLoader : GameManagerBase {
        public static ConfigsLoader Instance { get; private set; }
        public FlowfieldConfig FlowfieldConfig { get; private set; }
        public TerrainData TerrainData { get; private set; }
        public EnemiesSpawnerConfig EnemiesSpawnerConfig { get; private set; }
        public EnemyStatsConfig EnemyStatsConfig { get; private set; }
        public HumanBaseConfig HumanBaseConfig { get; private set; }
        
        private World _world => World.DefaultGameObjectInjectionWorld;
        
        public override void OnAwake() {
            LoadAddressableConfigs();
            InjectConfigs();
            Instance = this;
        }
        
        private void LoadAddressableConfigs() {
            var flowfieldHandle = Addressables.LoadAssetAsync<FlowfieldConfig>("config_flowfieldConfig");
            flowfieldHandle.WaitForCompletion();
            FlowfieldConfig = flowfieldHandle.Result;

            var terrainDataHandle = Addressables.LoadAssetAsync<TerrainData>("config_defaultTerrainData");
            terrainDataHandle.WaitForCompletion();
            TerrainData = terrainDataHandle.Result;

            var spawnerHandle = Addressables.LoadAssetAsync<EnemiesSpawnerConfig>("config_defaultEnemiesSpawnerConfig");
            spawnerHandle.WaitForCompletion();
            EnemiesSpawnerConfig = spawnerHandle.Result;

            var enemyStatsHandle = Addressables.LoadAssetAsync<EnemyStatsConfig>("config_defaultEnemyStatsConfig");
            enemyStatsHandle.WaitForCompletion();
            EnemyStatsConfig = enemyStatsHandle.Result;

            var humanBaseHandle = Addressables.LoadAssetAsync<HumanBaseConfig>("config_defaultHumanBaseConfig");
            humanBaseHandle.WaitForCompletion();
            HumanBaseConfig = humanBaseHandle.Result;
        }

        private void InjectConfigs() {
            _world.GetOrCreateSystem<FlowfieldManagerSystem>().InjectConfigs(FlowfieldConfig, TerrainData);
            _world.GetOrCreateSystem<SetAttackingStateSystem>().InjectConfigs(EnemyStatsConfig);
            _world.GetOrCreateSystem<EnemiesRotationSystem>().InjectConfigs(EnemyStatsConfig);
            _world.GetOrCreateSystem<SpawnEnemiesSystem>().InjectConfigs(EnemiesSpawnerConfig);
            _world.GetOrCreateSystem<MoveEnemiesSystem>().InjectConfigs(EnemyStatsConfig);
            _world.GetOrCreateSystem<HumanBaseHealthControllerSystem>().InjectConfigs(HumanBaseConfig);
        }
    }
}