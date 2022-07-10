using System.Collections.Generic;
using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Game.Ecs.Utils;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems.Spawners {
    public partial class TurretsParticlesPlayerSystem : SystemBase {
        private TurretsConfig _turretsConfig;
        private GameObject _particlesPrefab;
        
        private readonly Dictionary<Entity, ParticleSystem> _attachedParticles = new Dictionary<Entity, ParticleSystem>();

        protected override void OnCreate() {
            _turretsConfig = AddressablesLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
            _particlesPrefab = AddressablesLoader.Get<GameObject>(AddressablesConsts.MuzzelFlashParticles);
        }

        protected override void OnUpdate() {
            var ltwData = GetComponentDataFromEntity<LocalToWorld>(true);
            var offset = _turretsConfig.ParticlesOffset;

            Entities.WithAll<Tag_Turret>().ForEach((in TurretMuzzelFlashAnchorComponent anchorEntity, in TurretStateComponent state) => {
                if (!ltwData.HasComponent(anchorEntity.Value)) return;
                
                var ltw = ltwData[anchorEntity.Value];
                var particles = GetAttachedParticles(anchorEntity.Value);
                
                if (state.CurrentState == TurretState.Attacking) {
                    particles.PlaySafe(true);
                    particles.transform.SetAsChildOfEntityWithOffset(ltw, offset);
                } else if (state.CurrentState != TurretState.Attacking) {
                    particles.StopSafe(true);
                }
            }).WithoutBurst().WithReadOnly(ltwData).Run();
        }

        private ParticleSystem GetAttachedParticles(Entity entity) {
            if (_attachedParticles.TryGetValue(entity, out var particles)) {
                return particles;
            }
            var newParticles = Object.Instantiate(_particlesPrefab).GetComponent<ParticleSystem>();
            _attachedParticles.Add(entity, newParticles);
            return newParticles;
        }
    }
}