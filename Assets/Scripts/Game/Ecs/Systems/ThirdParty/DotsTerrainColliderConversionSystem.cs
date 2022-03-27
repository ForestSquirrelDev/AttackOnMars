using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;
using Collider = Unity.Physics.Collider;
using TerrainCollider = Unity.Physics.TerrainCollider;

// https://github.com/DOTS-Discord/Unity-DOTS-Discord/wiki/Authoring-to-create-DOTS-Physics-colliders-from-standard-unity-terrains

namespace Game.Ecs.Systems.ThirdParty {
    [Unity.Entities.ConverterVersion("Lukas", 6)]
    [Unity.Entities.UpdateAfter(typeof(PhysicsBodyConversionSystem))]
    public class DotsTerrainColliderConversionSystem : GameObjectConversionSystem {
        protected override unsafe void OnUpdate() {
            Entities.ForEach((DotsTerrainCollider physicsTerrain) => {
                var collisionFilter = new CollisionFilter {
                    BelongsTo = physicsTerrain.belongsTo.Value,
                    CollidesWith = physicsTerrain.collidesWith.Value,
                    GroupIndex = physicsTerrain.groupIndex
                };

                var ent = GetPrimaryEntity(physicsTerrain);

                var treeColliders = CreateTreeColliderInstances(physicsTerrain.terrainData, this);
                var terrainCollider = CreateTerrainCollider(physicsTerrain.terrainData, physicsTerrain.collisionMethod,
                    collisionFilter);

                var blobSize = physicsTerrain.treeCompoundColliderSize;
                var blobsToCreate = Mathf.CeilToInt(treeColliders.Length / (float)blobSize);

                for (int i = 0; i < blobsToCreate; i++) {
                    var treeEntity = this.CreateAdditionalEntity(physicsTerrain);

#if UNITY_EDITOR
                    DstEntityManager.SetName(treeEntity, physicsTerrain.name + $"_TreeCompoundCollider_{i + 1}");
#endif
                    var amountOfTreeColliders = math.min(blobSize, treeColliders.Length - i * blobSize);

                    var blobInstances =
                        new NativeArray<CompoundCollider.ColliderBlobInstance>(amountOfTreeColliders, Allocator.TempJob);

                    var source = (byte*)treeColliders.GetUnsafePtr() +
                                 i * blobSize * UnsafeUtility.SizeOf<CompoundCollider.ColliderBlobInstance>();

                    UnsafeUtility.MemCpy(blobInstances.GetUnsafePtr(), source,
                        blobInstances.Length * UnsafeUtility.SizeOf<CompoundCollider.ColliderBlobInstance>());

                    DstEntityManager.AddComponentData(treeEntity, new Translation() {
                        Value = physicsTerrain.transform.position
                    });

                    DstEntityManager.AddComponentData(treeEntity, new Rotation() {
                        Value = physicsTerrain.transform.rotation
                    });

                    DstEntityManager.AddComponentData(treeEntity, new PhysicsCollider() {
                        Value = CompoundCollider.Create(blobInstances)
                    });
                    blobInstances.Dispose();
                }


                DstEntityManager.AddComponentData(ent, new PhysicsCollider() {
                    Value = terrainCollider
                });

                DstEntityManager.RemoveComponent<DotsTerrainCollider>(ent);

                treeColliders.Dispose();
            });
        }


        private static NativeList<CompoundCollider.ColliderBlobInstance> CreateTreeColliderInstances(
            TerrainData terrainData, GameObjectConversionSystem conversionSystem
        ) {
            var prototypes = terrainData.treePrototypes;

            var prefabs = new List<Entity>();

            foreach (var prototype in prototypes) {
                var prefab = conversionSystem.GetPrimaryEntity(prototype.prefab);

                if (prefab == Entity.Null)
                    Debug.LogException(new Exception("Failed to resolve Tree Prototype"));

                prefabs.Add(prefab);

            }

            var instances =
                new NativeList<CompoundCollider.ColliderBlobInstance>(terrainData.treeInstances.Length,
                    Allocator.TempJob);

            foreach (var tree in terrainData.treeInstances) {
                var treePosition = (float3)tree.position * terrainData.size;

                if (!conversionSystem.DstEntityManager.HasComponent<PhysicsCollider>(prefabs[tree.prototypeIndex]))
                    continue;

                var collider =
                    conversionSystem.DstEntityManager.GetComponentData<PhysicsCollider>(prefabs[tree.prototypeIndex]);

                if (!collider.IsValid)
                    continue;

                instances.Add(new CompoundCollider.ColliderBlobInstance() {
                    Collider = collider.Value,
                    CompoundFromChild = new RigidTransform(Quaternion.Euler(0, tree.rotation, 0), treePosition)
                });
            }

            return instances;
        }

        private static BlobAssetReference<Collider> CreateTerrainCollider(
            TerrainData terrainData,
            TerrainCollider.CollisionMethod collisionMethod, CollisionFilter filter
        ) {
            var size = new int2(terrainData.heightmapResolution, terrainData.heightmapResolution);
            var scale = terrainData.heightmapScale;

            var colliderHeights = new NativeArray<float>(terrainData.heightmapResolution * terrainData.heightmapResolution,
                Allocator.TempJob);

            var terrainHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
                terrainData.heightmapResolution);


            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++) {
                var height = terrainHeights[x, y];
                colliderHeights[y + x * size.x] = height;
            }

            BlobAssetReference<Collider> collider = TerrainCollider.Create(colliderHeights, size, scale, collisionMethod, filter);
            colliderHeights.Dispose();
            return collider;
        }
    }
}
