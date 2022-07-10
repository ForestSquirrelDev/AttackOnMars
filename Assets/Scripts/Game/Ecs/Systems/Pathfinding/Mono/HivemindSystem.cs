using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Pathfinding;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Ecs.Systems.Pathfinding {
    public partial class HivemindSystem : SystemBase {
        public void Init() {
            var hiveMindTargetSingleton = EntityManager.CreateEntity(typeof(CurrentHivemindTargetSingleton));
            var humanBaseSingleton = GetSingletonEntity<Tag_MainHumanBase>();
            var humanBaseMatrix = EntityManager.GetComponentData<LocalToWorld>(humanBaseSingleton);
            EntityManager.SetComponentData(hiveMindTargetSingleton, new CurrentHivemindTargetSingleton {Value = humanBaseMatrix.Position});
        }
    
        protected override void OnUpdate() { }
    }
}
