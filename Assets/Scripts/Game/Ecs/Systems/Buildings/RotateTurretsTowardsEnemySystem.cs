using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Ecs.Systems.Spawners {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class RotateTurretsTowardsEnemySystem : SystemBase {
        private TurretsConfig _turretsConfig;
        
        private const float _lerpFramesCount = 5;
        private float _elapsedFrames;

        protected override void OnCreate() {
            _turretsConfig = ConfigsLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
        }

        protected override void OnUpdate() {
            if (_elapsedFrames >= _lerpFramesCount) _elapsedFrames = 0;
            var rotationSpeed = _turretsConfig.BaseRotationSpeed;
            var t = _elapsedFrames / _lerpFramesCount;
            var delta = UnityEngine.Time.deltaTime;
            
            var localToWorldData = GetComponentDataFromEntity<LocalToWorld>(true);
            var rotationData = GetComponentDataFromEntity<Rotation>(false);
            
            Dependency = Entities.WithAll<Tag_Turret>().ForEach((in CurrentTurretTargetComponent currentTargetComponent, in RotatableTurretPartsReferenceComponent rotatable, in Entity entity, 
                in CurrentTurretStateComponent state) => {
                if (currentTargetComponent.Entity == Entity.Null) return;
                if (state.Value != TurretState.ReadyToAttack && state.Value != TurretState.Attacking) return;
                
                var rotatableLtw = localToWorldData[rotatable.BaseRotation];
                var lookAtPoint = currentTargetComponent.Ltw.Position;
                var directionToWorldPoint = lookAtPoint - rotatableLtw.Position;
                var lookRotation = quaternion.LookRotation(directionToWorldPoint, new float3(0, 1, 0));
                var lerpedRotation = math.nlerp(rotatableLtw.Rotation, lookRotation, t * delta * rotationSpeed);

                rotationData[rotatable.BaseRotation] = new Rotation{Value = lerpedRotation};
            }).WithReadOnly(localToWorldData).Schedule(Dependency);

            _elapsedFrames++;
        }
    }
}
