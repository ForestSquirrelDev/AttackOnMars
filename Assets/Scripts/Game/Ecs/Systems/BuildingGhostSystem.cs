using Game.Ecs.Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public class BuildingGhostSystem : ComponentSystem {
        protected override void OnUpdate() {
            Entities.WithAll<Tag_BuildingGhost>().ForEach((ref LocalToWorld localToWorld, ref Translation translation) => {
                Matrix4x4 matrix = Matrix4x4.identity;
                matrix.SetColumn(0, localToWorld.Right.ToVector4());
                matrix.SetColumn(1, localToWorld.Up.ToVector4());
                matrix.SetColumn(2, localToWorld.Forward.ToVector4());
                matrix.SetColumn(3, localToWorld.Position.ToVector4());
                
                Vector3 mouse = InputUtility.MouseToWorld(Camera.main);
                Vector3 grid = BuildingGrid.WorldToGridCentered(mouse);
                Debug.Log($"Mouse: {mouse}, grid: {grid}");
                translation.Value = grid;
            });
        }
    }
}