using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Game.Ecs.Flowfield.Systems {
    public class FlowfieldJobDependenciesHandler {
        private NativeList<FrameBoundJobHandle> _globalFlowFieldDependencies;

        public void OnCreate() {
            _globalFlowFieldDependencies = new NativeList<FrameBoundJobHandle>(10, Allocator.Persistent);
        }
        
        public void OnUpdate() {
            Debug.Log($"System.OnUpdate");
            for (int i = 0; i < _globalFlowFieldDependencies.Length; i++) {
                var deps = _globalFlowFieldDependencies[i];
                if (deps.Completed) {
                    Debug.Log($"Deps count: {_globalFlowFieldDependencies.Length}");
                    _globalFlowFieldDependencies.RemoveAt(i);
                    Debug.Log($"Remove dependencies. Deps count: {_globalFlowFieldDependencies.Length}");
                }
            }
            for (var i = 0; i < _globalFlowFieldDependencies.Length; i++) {
                var deps = _globalFlowFieldDependencies[i];
                deps.SetLifetime(deps.FramesLifetime - 1);
                if (deps.FramesLifetime <= 0) {
                    deps.Complete();
                    deps.SetCompleted(true);
                }
                _globalFlowFieldDependencies[i] = deps;
            }
        }

        public void OnDestroy() {
            for (var i = 0; i < _globalFlowFieldDependencies.Length; i++) {
                var globalFlowFieldDependency = _globalFlowFieldDependencies[i];
                globalFlowFieldDependency.Handle.Complete();
            }
            _globalFlowFieldDependencies.Dispose();
        }
        
        public JobHandle ScheduleReadWrite<T>(T readWriteFlowfieldJob, int framesLifetime = 4) where T: struct, IJob {
            var dependencies = GetReadWriteDependenciesNativeArray();
            var currentDependencies = JobHandle.CombineDependencies(dependencies);
            var handle = readWriteFlowfieldJob.Schedule(currentDependencies);
            _globalFlowFieldDependencies.Add(new FrameBoundJobHandle {Handle = handle, FramesLifetime = framesLifetime});
            dependencies.Dispose();
            return handle;
        }

        public void CompleteAll() {
            foreach (var deps in _globalFlowFieldDependencies) {
                deps.Complete();
            }
        }

        private NativeArray<JobHandle> GetReadWriteDependenciesNativeArray() {
            var dependencies = new NativeArray<JobHandle>(_globalFlowFieldDependencies.Length, Allocator.Temp);
            for (var i = 0; i < _globalFlowFieldDependencies.Length; i++) {
                var deps = _globalFlowFieldDependencies[i];
                dependencies[i] = deps.Handle;
            }
            return dependencies;
        }

        private struct FrameBoundJobHandle {
            public JobHandle Handle;
            public int FramesLifetime;
            public bool Completed;

            public int SetLifetime(int lifetime) {
                FramesLifetime = lifetime;
                return FramesLifetime;
            }

            public void Complete() {
                Handle.Complete();
            }

            public void SetCompleted(bool value) {
                Completed = value;
            }
        }
    }
}