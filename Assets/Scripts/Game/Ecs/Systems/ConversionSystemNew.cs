using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems {
    public class ConversionSystemNew : MonoBehaviour, IDeclareReferencedPrefabs
    {
        
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
            referencedPrefabs.Add();
        }
    }
}