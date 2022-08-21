using Game.Ecs.Systems.Pathfinding;
using Unity.Entities;
using UnityEngine;
using Utils;
using Utils.Pathfinding;

public class FlowfieldDebugger : MonoBehaviour {
    private FlowfieldManagerSystem _flowfieldManagerSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FlowfieldManagerSystem>();

    private unsafe void Update() {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0)) {
            var ray = InputUtility.MouseToWorld(Camera.main);
            var flowfieldData = _flowfieldManagerSystem.FlowfieldRuntimeData;
            var parentCells = _flowfieldManagerSystem.ParentFlowFieldCells.AsParallelWriter();
            var parentCellIndex = FlowfieldUtility.CalculateIndexFromWorld(ray, flowfieldData.ParentGridOrigin, flowfieldData.ParentGridSize, flowfieldData.ParentCellSize);
            var parentCell = parentCells.ListData->Ptr[parentCellIndex];
            if (parentCell.ChildCells.ListData->IsEmpty) {
                Debug.Log($"Parent cell {parentCell.WorldCenter}. Empty child cells");
                return;
            }
            Debug.Log($"Parent cell {parentCell.GridPosition}. Unwalkable: {parentCell.Unwalkable}. Best/bast cost: {parentCell.BestCost}/{parentCell.BaseCost}");
            var childCellIndex = FlowfieldUtility.CalculateIndexFromWorld(ray, parentCell.WorldPosition, flowfieldData.ChildGridSize, flowfieldData.ChildCellSize);
            var childCell = parentCell.ChildCells.ListData->Ptr[childCellIndex];
            Debug.Log($"Child cell. World center: {childCell.WorldCenter}. Grid position: {childCell.GridPosition}. Index: {childCellIndex}." +
                      $"Flowfield direction: {childCell.BestFlowfieldDirection}. Entities count: {childCell.Entities.Count()}. Best/base cost: {childCell.BestCost}/{childCell.BaseCost}." +
                      $"Unwalkable: {childCell.Unwalkable}");
        }
    }
}
