using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using Game.Ecs.Flowfield.Components;
using Game.Ecs.Flowfield.Systems;
using Game.Ecs.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;

namespace Game.Ecs.Flowfield {
    public class FlowfieldGizmosDrawer : MonoBehaviour {
        [SerializeField] private Terrain _terrain;

        [SerializeField] private bool _initializeOnStart = true;
        [SerializeField] private bool _debugCosts = true;
        [SerializeField] private bool _drawArrows = true;
        [SerializeField] private bool _debugPositions;
        [SerializeField] private bool _debugSmallGrids = true;
        [SerializeField] private bool _debugParentGrid = true;

         private FlowfieldManagerSystem _flowfieldManagerSystem;
         private NativeList<FlowfieldCellComponent> _flowfieldCells;
         private List<FlowfieldCellComponent> _copiedResults = new List<FlowfieldCellComponent>();

         private void Awake() {
             _flowfieldManagerSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FlowfieldManagerSystem>();
             _flowfieldCells = new NativeList<FlowfieldCellComponent>(Allocator.Persistent);
         }

         [Button]
         private void Start() {
             if (_initializeOnStart)
                StartCoroutine(UpdateCellsRoutine());
         }

         private IEnumerator UpdateCellsRoutine() {
             _flowfieldCells.Clear();
             while (Application.isPlaying) {
                 if (_flowfieldManagerSystem.Initialized == false) {
                     yield return new WaitForEndOfFrame();
                     continue;
                 }
                 yield return WaitForFixedFramesCount(1);
                 _copiedResults.Clear();
                 foreach (var cell in _flowfieldCells) {
                     _copiedResults.Add(cell);
                 }
                 StartFillCellsJob();
             }
         }

         private IEnumerator WaitForFixedFramesCount(int framesCount) {
             for (int i = 0; i < framesCount; i++) {
                 yield return null;
             }
         }
         
         private void StartFillCellsJob() {
             var fillDebugCellsJob = new FillCellsForDebugJob {
                 FlowFieldCellsIn = _flowfieldManagerSystem.ParentFlowFieldCells.AsParallelReader(),
                 FlowFieldCellsOut = _flowfieldCells
             };
             _flowfieldManagerSystem.ScheduleReadOnly(fillDebugCellsJob);
         }

         private void OnDrawGizmos() {
             if (!Application.isPlaying || !_flowfieldManagerSystem.Initialized) return;
             if (_debugParentGrid) {
                 foreach (var cell in _copiedResults) {
                     DebugCell(cell, 15f, 2.5f, true);
                 }       
             }
         }

         private void OnDestroy() {
             _flowfieldCells.Dispose();
         }

         private void DebugCell(FlowfieldCellComponent cell, float arrowLength, float arrowThickness, bool isParentCell) {
             Gizmos.color = cell.BaseCost.Approximately(float.MaxValue) ? Color.red : Color.green;
             DrawSingleCell(cell, cell.Size, true);
        
             if (_debugCosts) {
                 DrawCosts(cell);
             }
             if (_drawArrows && !(cell.BestDirection.x == 0 && cell.BestDirection.y == 0) && cell.BestCost != float.MaxValue) {
                 DrawSingleArrow(cell, arrowLength, arrowThickness);
             }
             if (_debugPositions) {
                 var text = cell.GridPosition.ToString();
                 Handles.Label(cell.WorldPosition, text);
             }
             // if (cell.ChildCells != null && _debugSmallGrids) {
             //     foreach (var childCell in cell.ChildCells) {
             //         DebugCell(childCell, 1f, 1f);
             //     }
             // }
         }
        
         private void DrawSingleArrow(FlowfieldCellComponent cell, float length = 1f, float thickness = 1f) {
             Handles.color = Color.cyan;
             var arrowTip = new Vector3(cell.WorldCenter.x + cell.BestDirection.x * length, cell.WorldCenter.y, cell.WorldCenter.z + cell.BestDirection.y * length);
             Handles.DrawLine(cell.WorldCenter, arrowTip, thickness);
             var middle = ((Vector3)cell.WorldCenter + arrowTip) * 0.5f;
             var AB = (Vector3)cell.WorldCenter - arrowTip;
             var middleRight = Vector3.Cross(Vector3.down, AB).normalized * length / 3;
             var middleLeft = Vector3.Cross(AB, Vector3.down).normalized * length / 3;
             Handles.DrawLine(arrowTip, (Vector3)middle + middleRight, thickness);
             Handles.DrawLine(arrowTip, (Vector3)middle + middleLeft, thickness);
         }
        
         private void DrawCosts(FlowfieldCellComponent cell) {
             Handles.color = Color.magenta;
        
             var bestCost = "Best cost:" + ((cell.BaseCost == float.MaxValue || cell.BestCost == float.MaxValue) ? "MAX" : (cell.BestCost).ToString("0"));
             var baseCost = "Base cost:" + (cell.BaseCost == float.MaxValue || cell.BestCost == float.MaxValue ? "MAX" : cell.BaseCost.ToString("0"));
             Handles.Label(cell.WorldCenter, bestCost);
             Handles.Label(cell.WorldPosition, baseCost);
         }

         private void DrawSingleCellSimple(FlowfieldCellComponent cell, float sphereSize, bool takeHeightIntoAccount = true) {
             var worldPosition = takeHeightIntoAccount ? (Vector3)cell.WorldCenter : new Vector3(cell.WorldCenter.x, 0f, cell.WorldCenter.z);
             Gizmos.color = Color.grey;
             Gizmos.DrawSphere(worldPosition, sphereSize);
         }
        
         private void DrawSingleCell(FlowfieldCellComponent cell, float cellSize, bool takeHeightIntoAccount = true) {
             if (cell.IsBestChildCell)
                 Gizmos.color = Color.magenta;
             var height = takeHeightIntoAccount ? cell.WorldPosition.y : _terrain.transform.position.y;
             cell.WorldPosition.y = height;
             var rightBottom = new Vector3(cell.WorldPosition.x + cellSize, height, cell.WorldPosition.z);
             var rightTop = new Vector3(cell.WorldPosition.x + cellSize, height, cell.WorldPosition.z + cellSize);
             var leftTop = new Vector3(cell.WorldPosition.x, height, cell.WorldPosition.z + cellSize);
             Gizmos.DrawLine(cell.WorldPosition, rightBottom);
             Gizmos.DrawLine(rightBottom, rightTop);
             Gizmos.DrawLine(rightTop, leftTop);
             Gizmos.DrawLine(leftTop, cell.WorldPosition);
         }
         
         private struct FillCellsForDebugJob : IJob {
             public UnsafeList<FlowfieldCellComponent>.ParallelReader FlowFieldCellsIn;
             public NativeList<FlowfieldCellComponent> FlowFieldCellsOut;

             public void Execute() {
                 for (var i = 0; i < FlowFieldCellsIn.Length; i++) {
                     unsafe {
                         var cell = FlowFieldCellsIn.Ptr[i];
                         if (FlowFieldCellsOut.Length < FlowFieldCellsIn.Length) {
                             FlowFieldCellsOut.Add(cell);
                         } else {
                             FlowFieldCellsOut[i] = cell;
                         }
                     }
                 }
             }
         }
    }
}