using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Utils.Maths;

namespace Game.Ecs.Systems.Spawners {
    public partial class RotateTurretsTowardsEnemySystem : SystemBase {
        private TurretsConfig _turretsConfig;

        protected override void OnCreate() {
            _turretsConfig = AddressablesLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
        }

        protected override void OnUpdate() {
            var rotationSpeed = _turretsConfig.BaseRotationSpeed;
            var t = Time.DeltaTime;

            var localToWorldData = GetComponentDataFromEntity<LocalToWorld>(true);
            var rotationData = GetComponentDataFromEntity<Rotation>(false);
            Entities.WithAll<Tag_Turret>().ForEach((ref RotatableTurretPartsReferenceComponent rotatable, in CurrentTurretTargetComponent currentTargetComponent, in Entity entity, 
                in TurretStateComponent state) => {
                if (currentTargetComponent.Entity == Entity.Null) return;
                if (state.CurrentState != TurretState.ReadyToAttack && state.CurrentState != TurretState.Attacking) return;

                var rotatableLtw = localToWorldData[rotatable.BaseRotation];
                var baseRotation = rotationData[rotatable.BaseRotation];
                var lookAtPoint = currentTargetComponent.Ltw.Position;
                var directionToWorldPoint = lookAtPoint - rotatableLtw.Position;
                var lookRotation = Quaternion.LookRotation(directionToWorldPoint.Normalize());
                var rotateTowards = Quaternion.RotateTowards(baseRotation.Value, lookRotation, t * rotationSpeed);
                
                rotationData[rotatable.BaseRotation] = new Rotation{Value = rotateTowards};
            }).WithReadOnly(localToWorldData).Schedule();
        }
    }
}
