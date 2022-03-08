using System;
using System.Collections.Generic;
using EasyButtons;
using Game.Ecs.Systems.Bridge.GlobalGrid;
using UnityEngine;
using Utils;

namespace Editor {
    public class MonoGridDrawer : MonoBehaviour {
        private MonoGridManager _manager;
        private int _width, _height;
        private float _cellSize;
        private Vector3 _parentPos;
        private List<Tuple<Vector3, Vector3>> _linePositions = new List<Tuple<Vector3, Vector3>>();
        
        private void OnDrawGizmos() {
            _manager ??= GetComponent<MonoGridManager>();
            if (_manager.Width != _width || _manager.Height != _height || _manager.CellSize != _cellSize || _manager.transform.position != _parentPos) {
                if (!Application.isPlaying) _manager.InitEditorGrid();
                UpdateGrid();
            }
            DrawGrid();
        }

        private void OnValidate() {
            _manager ??= GetComponent<MonoGridManager>();
            _manager.InitEditorGrid();
            UpdateGrid();
        }

        [Button]
        private void UpdateGrid() {
            _width = _manager.Width;
            _height = _manager.Height;
            _cellSize = _manager.CellSize;
            _parentPos = _manager.transform.position;
            _linePositions.Clear();
            
            for (int x = 0; x < _manager.Width; x++) {
                for (int z = 0; z < _manager.Height; z++) {
                    Vector3 worldPos = _manager.EditorGrid.GridToWorld(new Vector3(x, 0, z).RoundToVector2IntXZ(), false);
                    worldPos.y = _manager.Terrain.SampleHeight(worldPos);
                    Vector3 targetZ = new Vector3(worldPos.x, worldPos.y, worldPos.z + _manager.CellSize);
                    targetZ.y = _manager.Terrain.SampleHeight(targetZ);
                    Vector3 targetX = new Vector3(worldPos.x + _manager.CellSize, worldPos.y, worldPos.z);
                    targetX.y = _manager.Terrain.SampleHeight(targetX);
                    _linePositions.Add(new Tuple<Vector3, Vector3>(worldPos, targetZ));
                    _linePositions.Add(new Tuple<Vector3, Vector3>(worldPos, targetX));
                }
            }
            
            Vector3 rightBottomCorner = _manager.EditorGrid.GridToWorld(new Vector3(_width, 0, 0).RoundToVector2IntXZ(), false);
            rightBottomCorner.y = _manager.Terrain.SampleHeight(rightBottomCorner);
            Vector3 rightTopCorner = _manager.EditorGrid.GridToWorld(new Vector3(_width, 0, _height).RoundToVector2IntXZ(), false);
            rightTopCorner.y = _manager.Terrain.SampleHeight(rightTopCorner);
            Vector3 leftTopCorner = _manager.EditorGrid.GridToWorld(new Vector3(0, 0, _height).RoundToVector2IntXZ(), false);
            leftTopCorner.y = _manager.Terrain.SampleHeight(leftTopCorner);
            _linePositions.Add(new Tuple<Vector3, Vector3>(rightBottomCorner, rightTopCorner));
            _linePositions.Add(new Tuple<Vector3, Vector3>(leftTopCorner, rightTopCorner));
        }
        
        private void DrawGrid() {
            foreach (var direction in _linePositions) {
                Gizmos.DrawLine(direction.Item1, direction.Item2);
            }
        }
    }
}