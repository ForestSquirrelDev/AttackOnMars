using Game.AddressableConfigs;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Utils.Maths;

namespace Game.Ecs.Systems {
    public partial class RotateEnemiesOnFlowfieldSystem : SystemBase {
        private EnemyStatsConfig _config;
        
        private const float _lerpFramesCount = 5;
        private float _elapsedFrames = 0;

        protected override void OnCreate() {
            _config = AddressablesLoader.Get<EnemyStatsConfig>(AddressablesConsts.DefaultEnemyStatsConfig);
        }

        protected override void OnUpdate() {
            if (_elapsedFrames >= _lerpFramesCount) _elapsedFrames = 0;
            var rotationSpeed = _config.RotationSpeed;
            var t = _elapsedFrames / _lerpFramesCount;
            var delta = UnityEngine.Time.deltaTime;
            var basePosition = GetSingleton<CurrentHivemindTargetSingleton>().Value;

            Entities.WithAll<Tag_Enemy>().ForEach((ref Rotation rotation, in LocalToWorld ltw, in BestEnemyGridDirectionComponent bestDirection, in EnemyStateComponent enemyState) => {
                if (bestDirection.Value.Magnitude() <= 0.01f) return;
                if (enemyState.Value == EnemyState.Attacking) return;
                
                var lookAtPoint = enemyState.Value != EnemyState.Attacking ? ltw.Position + bestDirection.Value : ltw.Position + math.normalizesafe(basePosition - ltw.Position);
                var directionToWorldPoint = lookAtPoint - ltw.Position;
                var lookRotation = quaternion.LookRotation(directionToWorldPoint, new float3(0, 1, 0));
                var lerpedRotation = math.nlerp(ltw.Rotation, lookRotation, t * delta * rotationSpeed);
                rotation.Value = lerpedRotation;
            }).ScheduleParallel();
            
            _elapsedFrames++;
        }
    }
}
