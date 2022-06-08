using Game.Ecs.Components.Pathfinding;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Experiments {
    public struct LongLongJob : IJob {
        private int _i;

        public LongLongJob(int i) {
            _i = i;
        }
        
        public void Execute() {
            for (int i = 0; i < _i; i++) {
                var PerlinNoiseKekw = Mathf.PerlinNoise(_i, i);
                for (int k = 0; k < _i; k++) {
                    var PerlinNoiseKekw1 = Mathf.PerlinNoise(_i, i);
                    for (int kekw = 0; kekw < _i; kekw++) {
                        var PerlinNoiseKekw2 = Mathf.PerlinNoise(_i, i);
                        _i++;
                    }
                }
            }
            Debug.Log($"Wow bro that was a long job. Hope you not get tired waiting.");
        }
    }
    public struct AddToUnsafeListJob : IJob {
        private UnsafeList<FlowfieldCellComponent>.ParallelReader _reader;
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _writer;

        public AddToUnsafeListJob(UnsafeList<FlowfieldCellComponent>.ParallelReader reader, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer) {
            _reader = reader;
            _writer = writer;
        }
        
        public void Execute() {
            _writer.AddNoResize(new FlowfieldCellComponent{WorldPosition = new float3(5,5, 1)});
        }
    }
    public struct WriteToUnsafeListJob : IJob {
        private UnsafeList<FlowfieldCellComponent>.ParallelReader _reader;
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _writer;

        public WriteToUnsafeListJob(UnsafeList<FlowfieldCellComponent>.ParallelReader reader, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer) {
            _reader = reader;
            _writer = writer;
        }
        
        public unsafe void Execute() {
            _writer.ListData->Ptr[0] = new FlowfieldCellComponent { WorldPosition = new float3(6, 6, 2) };
        }
    }
    public struct PrintMessageJob : IJob {
        public UnsafeList<FlowfieldCellComponent>.ParallelWriter Writer;
        public unsafe void Execute() {
            for (int i = 0; i < Writer.ListData->Length; i++) {
                Debug.Log($"Cell {i}: {Writer.ListData->Ptr[i].WorldPosition}");
            }
        }
    }
}