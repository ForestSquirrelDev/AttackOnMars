using Unity.Entities;
using Unity.Jobs;
using Utils.Logger;

namespace Game.Ecs.Systems {
    public class TestJobSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            //CustomLogger.Log(World.GetExistingSystem<JobifiedPositioningQuadSystem>().publicResult[0].ToString(), LogOptions.Singleton);
            return new JobHandle();
        }
    }
}