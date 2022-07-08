using Unity.Collections;
using Unity.Jobs;

namespace Game.Ecs.Systems.Pathfinding {
    // Essentially this class schedules all jobs that i pass to it one-by-one: until previous bunch of scheduled jobs isn't completed, next one won't start.
    // Which virtually makes their work single threaded, or just "sequenced".
    // This indeed sucks, but at the same time it allows all jobs work with the same native collections and, at the same time,
    // avoid touching main thread completely for multiple frames, which literally grounds their impact on performance.
    // There are also cases when we still need to access job's results on the main thread, for instance in Gizmos drawer.
    // For that case all jobs have virtual "lifetime" based on frames: after given number of frames job.Complete() is called so it's results can be accessed from main thread.
    // That means if we want to access job's results, we could schedule it, for instance, with lifetime of one frame, and wait one frame before accessing results.
    public class DependenciesScheduler {
        private NativeList<FrameBoundJobHandle> _readWriteDependencies;
        private NativeList<FrameBoundJobHandle> _readonlyDependencies;

        public DependenciesScheduler() {
            _readWriteDependencies = new NativeList<FrameBoundJobHandle>(10, Allocator.Persistent);
            _readonlyDependencies = new NativeList<FrameBoundJobHandle>(10, Allocator.Persistent);
        }
        
        public void OnUpdate() {
            RemoveCompletedHandles();
            DecrementHandlesLifetimeAndComplete();
        }
        
        public void Dispose() {
            CompleteAll();
            _readWriteDependencies.Dispose();
            _readonlyDependencies.Dispose();
        }
        
        private void RemoveCompletedHandles() {
            for (int i = 0; i < _readWriteDependencies.Length; i++) {
                var deps = _readWriteDependencies[i];
                if (deps.Completed) {
                    _readWriteDependencies.RemoveAt(i);
                }
            }
            
            for (int i = 0; i < _readonlyDependencies.Length; i++) {
                var deps = _readonlyDependencies[i];
                if (deps.Completed) {
                    _readonlyDependencies.RemoveAt(i);
                }
            }
        }
        
        private void DecrementHandlesLifetimeAndComplete() {
            for (var i = 0; i < _readWriteDependencies.Length; i++) {
                var deps = _readWriteDependencies[i];
                deps.DecrementLifetime();
                if (deps.FramesLifetime <= 0) {
                    deps.Complete();
                }
                _readWriteDependencies[i] = deps;
            }
            
            for (var i = 0; i < _readonlyDependencies.Length; i++) {
                var deps = _readonlyDependencies[i];
                deps.DecrementLifetime();
                if (deps.FramesLifetime <= 0) {
                    deps.Complete();
                }
                _readonlyDependencies[i] = deps;
            }
        }

        public JobHandle ScheduleReadWrite<T>(T readWriteFlowfieldJob, int framesLifetime = 4, JobHandle dependenciesIn = default) where T : struct, IJob {
            var dependencies = GetDependenciesForReadWrite();
            var combinedDependencies = JobHandle.CombineDependencies(JobHandle.CombineDependencies(dependencies), dependenciesIn);
            var handle = readWriteFlowfieldJob.Schedule(combinedDependencies);
            _readWriteDependencies.Add(new FrameBoundJobHandle(handle, framesLifetime));
            dependencies.Dispose();
            return handle;
        }

        public JobHandle ScheduleReadOnly<T>(T readOnlyFlowfieldJob, int framesLifetime = 1) where T : struct, IJob {
            var dependencies = GetDependenciesForReadOnly();
            var combinedDependencies = JobHandle.CombineDependencies(dependencies);
            var handle = readOnlyFlowfieldJob.Schedule(combinedDependencies);
            _readonlyDependencies.Add(new FrameBoundJobHandle(handle, framesLifetime));
            dependencies.Dispose();
            return handle;
        }
        
        public void CompleteAll() {
            foreach (var deps in _readWriteDependencies) {
                deps.Complete();
            }
            foreach (var deps in _readonlyDependencies) {
                deps.Complete();
            }
        }

        public JobHandle GetCombinedReadWriteDependencies() {
            var deps = GetDependenciesForReadWrite();
            var combinedDependencies = JobHandle.CombineDependencies(deps);
            deps.Dispose();
            return combinedDependencies;
        }

        public void AddExternalReadWriteDependency(JobHandle dependency, int framesLifetime = 4) {
            _readWriteDependencies.Add(new FrameBoundJobHandle(dependency, framesLifetime));
        }

        public void AddExternalReadOnlyDependency(JobHandle dependency, int framesLifeTime = 1) {
            _readonlyDependencies.Add(new FrameBoundJobHandle(dependency, framesLifeTime));
        }

        private NativeArray<JobHandle> GetDependenciesForReadWrite() {
            var dependencies = new NativeArray<JobHandle>(_readWriteDependencies.Length + _readonlyDependencies.Length, Allocator.Temp);
            var foundReadWriteJobsCount = 0;
            for (var readWriteIndex = 0; readWriteIndex < _readWriteDependencies.Length; readWriteIndex++) {
                var deps = _readWriteDependencies[readWriteIndex];
                dependencies[readWriteIndex] = deps.Handle;
                foundReadWriteJobsCount++;
            }
            for (var readonlyIndex = 0; readonlyIndex < _readonlyDependencies.Length; readonlyIndex++) {
                var deps = _readonlyDependencies[readonlyIndex];
                dependencies[readonlyIndex + foundReadWriteJobsCount] = deps.Handle;
            }
            return dependencies;
        }

        private NativeArray<JobHandle> GetDependenciesForReadOnly() {
            var dependencies = new NativeArray<JobHandle>(_readWriteDependencies.Length, Allocator.Temp);
            for (var i = 0; i < _readWriteDependencies.Length; i++) {
                var deps = _readWriteDependencies[i];
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