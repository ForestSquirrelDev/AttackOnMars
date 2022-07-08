using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems.Spawners {
    public partial class TurretsParticlesManagerSystem : SystemBase {
        private TurretsConfig _turretsConfig;
        private GameObject _particlesPrefab;
        private ParticleSystem _particlesDebug;

        protected override void OnCreate() {
            _turretsConfig = AddressablesLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
            _particlesPrefab = AddressablesLoader.Get<GameObject>(AddressablesConsts.MuzzelFlashParticles);
            _particlesDebug = UnityEngine.Object.Instantiate(_particlesPrefab).GetComponent<ParticleSystem>();
            _particlesDebug.Play(true);
            //new CopyTransformToGameObject{}
        }

        protected override void OnUpdate() {
            var ltwData = GetComponentDataFromEntity<LocalToWorld>(true);
            var offset = _turretsConfig.ParticlesOffset;

            Entities.WithAll<Tag_Turret>().ForEach((in TurretMuzzelFlashAnchorComponent anchorEntity) => {
                if (!ltwData.HasComponent(anchorEntity.Value)) return;
                
                var ltw = ltwData[anchorEntity.Value];
                var pos = Matrix4x4Extensions.LocalOffsetToWorldPoint(ltw.Value, new float4(offset.xyz, 0));
                _particlesDebug.gameObject.transform.position = pos.xyz;
                _particlesDebug.gameObject.transform.localScale = ltw.Value.GetScale();
                _particlesDebug.transform.rotation = ltw.Rotation;
            }).WithoutBurst().WithReadOnly(ltwData).Run();
        }
    }
}