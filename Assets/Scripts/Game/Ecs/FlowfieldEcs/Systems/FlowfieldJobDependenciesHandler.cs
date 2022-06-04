using Unity.Collections;
using Unity.Jobs;

namespace Game.Ecs.Flowfield.Systems {
    // Essentially this class schedules all jobs that somehow interact with flowfield one-by-one: until previous bunch of scheduled jobs isn't completed, next one won't start.
    // Which virtually makes their work single threaded, or just "sequenced".
    // This indeed sucks, but at the same time it allows all flowfield-related jobs work with the same native collections and, at the same time,
    // avoid touching main thread completely for multiple frames, which literally grounds their impact on performance.
    // There are also cases when we still need to access job's results on the main thread, for instance in Gizmos drawer.
    // For that case all jobs have virtual "lifetime" based on frames: after given number of frames job.Complete() is called so it's results can be accessed from main thread.
    // That means if we want to access job's results, we could schedule it, for instance, with lifetime of one frame, and wait one frame before accessing results.
    public class FlowfieldJobDependenciesHandler {
        private NativeList<FrameBoundJobHandle> _readWriteFlowfieldDependencies;
        private NativeList<FrameBoundJobHandle> _readonlyFlowfieldDependencies;

        public void OnCreate() {
            _readWriteFlowfieldDependencies = new NativeList<FrameBoundJobHandle>(10, Allocator.Persistent);
            _readonlyFlowfieldDependencies = new NativeList<FrameBoundJobHandle>(10, Allocator.Persistent);
        }
        
        public void OnUpdate() {
            RemoveCompletedHandles();
            DecrementHandlesLifetimeAndComplete();
        }
        
        public void OnDestroy() {
            CompleteAll();
            _readWriteFlowfieldDependencies.Dispose();
            _readonlyFlowfieldDependencies.Dispose();
        }
        
        private void RemoveCompletedHandles() {
            for (int i = 0; i < _readWriteFlowfieldDependencies.Length; i++) {
                var deps = _readWriteFlowfieldDependencies[i];
                if (deps.Completed) {
                    _readWriteFlowfieldDependencies.RemoveAt(i);
                }
            }
            
            for (int i = 0; i < _readonlyFlowfieldDependencies.Length; i++) {
                var deps = _readonlyFlowfieldDependencies[i];
                if (deps.Completed) {
                    _readonlyFlowfieldDependencies.RemoveAt(i);
                }
            }
        }
        
        private void DecrementHandlesLifetimeAndComplete() {
            for (var i = 0; i < _readWriteFlowfieldDependencies.Length; i++) {
                var deps = _readWriteFlowfieldDependencies[i];
                deps.DecrementLifetime();
                if (deps.FramesLifetime <= 0) {
                    deps.Complete();
                }
                _readWriteFlowfieldDependencies[i] = deps;
            }
            
            for (var i = 0; i < _readonlyFlowfieldDependencies.Length; i++) {
                var deps = _readonlyFlowfieldDependencies[i];
                deps.DecrementLifetime();
                if (deps.FramesLifetime <= 0) {
                    deps.Complete();
                }
                _readonlyFlowfieldDependencies[i] = deps;
            }
        }

        public JobHandle ScheduleReadWrite<T>(T readWriteFlowfieldJob, int framesLifetime = 4, JobHandle dependenciesIn = default) where T : struct, IJob {
            var dependencies = GetDependenciesForReadWrite();
            var combinedDependencies = JobHandle.CombineDependencies(JobHandle.CombineDependencies(dependencies), dependenciesIn);
            var handle = readWriteFlowfieldJob.Schedule(combinedDependencies);
            _readWriteFlowfieldDependencies.Add(new FrameBoundJobHandle(handle, framesLifetime));
            dependencies.Dispose();
            return handle;
        }

        public JobHandle ScheduleReadOnly<T>(T readOnlyFlowfieldJob, int framesLifetime = 1) where T : struct, IJob {
            var dependencies = GetDependenciesForReadOnly();
            var combinedDependencies = JobHandle.CombineDependencies(dependencies);
            var handle = readOnlyFlowfieldJob.Schedule(combinedDependencies);
            _readonlyFlowfieldDependencies.Add(new FrameBoundJobHandle(handle, framesLifetime));
            dependencies.Dispose();
            return handle;
        }
        
        public JobHandle ScheduleNonPooled<T>(T job, JobHandle depenenciesIn) where T : struct, IJob {
            return job.Schedule(depenenciesIn);
        }

        public void CompleteAll() {
            foreach (var deps in _readWriteFlowfieldDependencies) {
                deps.Complete();
            }
            foreach (var deps in _readonlyFlowfieldDependencies) {
                deps.Complete();
            }
        }

        private NativeArray<JobHandle> GetDependenciesForReadWrite() {
            var dependencies = new NativeArray<JobHandle>(_readWriteFlowfieldDependencies.Length + _readonlyFlowfieldDependencies.Length, Allocator.Temp);
            var foundReadWriteJobsCount = 0;
            for (var readWriteIndex = 0; readWriteIndex < _readWriteFlowfieldDependencies.Length; readWriteIndex++) {
                var deps = _readWriteFlowfieldDependencies[readWriteIndex];
                dependencies[readWriteIndex] = deps.Handle;
                foundReadWriteJobsCount++;
            }
            for (var readonlyIndex = 0; readonlyIndex < _readonlyFlowfieldDependencies.Length; readonlyIndex++) {
                var deps = _readonlyFlowfieldDependencies[readonlyIndex];
                dependencies[readonlyIndex + foundReadWriteJobsCount] = deps.Handle;
            }
            return dependencies;
        }

        private NativeArray<JobHandle> GetDependenciesForReadOnly() {
            var dependencies = new NativeArray<JobHandle>(_readWriteFlowfieldDependencies.Length, Allocator.Temp);
            for (var i = 0; i < _readWriteFlowfieldDependencies.Length; i++) {
                var deps = _readWriteFlowfieldDependencies[i];
                dependencies[i] = deps.Handle;
            }
            return dependencies;
        }

        private struct FrameBoundJobHandle {
            public JobHandle Handle { get; }
            public int FramesLifetime { get; private set; }
            public bool Completed { get; private set; }

            public FrameBoundJobHandle(JobHandle handle, int framesLifetime) {
                Handle = handle;
                FramesLifetime = framesLifetime;
                Completed = false;
            }

            public void DecrementLifetime() {
                FramesLifetime--;
            }

            public void Complete() {
                Handle.Complete();
                Completed = true;
            }
        }
    }
}