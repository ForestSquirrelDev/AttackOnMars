using Game.AddressableConfigs;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils.Maths;

namespace Game.Ecs.Systems {
    public partial class EnemiesRotationSystem : SystemBase {
        private EnemyStatsConfig _config;
        
        private const float _lerpFramesCount = 5;
        private float _elapsedFrames = 0;

        public void InjectConfigs(EnemyStatsConfig config) {
            _config = config;
        }

        protected override void OnUpdate() {
            if (_elapsedFrames >= _lerpFramesCount) _elapsedFrames = 0;
            var rotationSpeed = _config.RotationSpeed;
            var t = _elapsedFrames / _lerpFramesCount;
            var delta = UnityEngine.Time.deltaTime;

            Entities.WithAll<Tag_Enemy>().ForEach((ref Rotation rotation, in LocalToWorld localToWorldMatrix, in BestEnemyDirectionComponent bestDirection) => {
                if (bestDirection.Value.Magnitude() <= 0.01f) return;
                var lookAtPoint = localToWorldMatrix.Position + bestDirection.Value;
                var directionToWorldPoint = (lookAtPoint - localToWorldMatrix.Position);
                var lookRotation = quaternion.LookRotation(directionToWorldPoint, new float3(0, 1, 0));
                var lerpedRotation = math.nlerp(localToWorldMatrix.Rotation, lookRotation, t * delta * rotationSpeed);
                rotation.Value = lerpedRotation;
            }).ScheduleParallel();
            
            _elapsedFrames++;
        }
    }
}
