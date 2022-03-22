using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Game.WeirdHacks {
    public class TestTest : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            dstManager.AddComponentData(entity, new CopyTransformFromGameObject());
        }
    }
}