using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems.Spawners {
    public class EnemySpawnPointsGizmosDrawer : MonoBehaviour {
        [SerializeField] private Color _color = Color.blue;
        private SpawnEnemiesSystem _spawnEnemiesSystem;

        private void Awake() {
            _spawnEnemiesSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SpawnEnemiesSystem>();
        }

        private void OnDrawGizmos() {
            Gizmos.color = _color;
            _spawnEnemiesSystem?.OnDrawGizmos();
        }
    }
}