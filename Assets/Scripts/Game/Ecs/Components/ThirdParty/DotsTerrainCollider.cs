using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;
using TerrainCollider = Unity.Physics.TerrainCollider;

//https://github.com/DOTS-Discord/Unity-DOTS-Discord/wiki/Authoring-to-create-DOTS-Physics-colliders-from-standard-unity-terrains

[DisallowMultipleComponent]
public class DotsTerrainCollider : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs {
    [Range(10, 2000), Tooltip("Trees are split into Compound Colliders containing this amount of instances")]
    public int treeCompoundColliderSize = 1900;

    public TerrainData terrainData;

    public PhysicsCategoryTags belongsTo;
    public PhysicsCategoryTags collidesWith;
    public int groupIndex;
    public TerrainCollider.CollisionMethod collisionMethod;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        conversionSystem.DeclareAssetDependency(this.gameObject, terrainData);
        dstManager.AddComponentObject(entity, this);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
        referencedPrefabs.AddRange(terrainData.treePrototypes.Select(i => i.prefab));
    }
}
