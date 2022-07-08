using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Ecs.Systems.Spawners {
    public partial class RotateTurretsBarrelSystem : SystemBase {
        private TurretsConfig _turretsConfig;

        protected override void OnCreate() {
            _turretsConfig = AddressablesLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
        }

        protected override void OnUpdate() {
            var rotationData = GetComponentDataFromEntity<Rotation>();
            var increaseStep = _turretsConfig.BarrelRotationSpeedIncreateStep;
            var maxSpeed = _turretsConfig.BarrelMaxRotationSpeed;
            Entities.WithAll<Tag_Turret>().ForEach((ref TurretBarrelCurrentRotationComponent rotationComponent, in RotatableTurretPartsReferenceComponent rotatable, in CurrentTurretStateComponent state) => {
                if (!rotationData.HasComponent(rotatable.Barrel)) return;
                
                rotationComponent.CurrentSpeed = state.Value != TurretState.Attacking 
                    ? math.clamp(rotationComponent.CurrentSpeed - increaseStep, 0, maxSpeed) 
                    : math.clamp(rotationComponent.CurrentSpeed + increaseStep, 0, maxSpeed);
                rotationComponent.Angle += rotationComponent.CurrentSpeed;
                
                var newRotation = quaternion.RotateZ(rotationComponent.Angle);
                rotationData[rotatable.Barrel] = new Rotation{Value = newRotation};

            }).Schedule();
        }
    }
}