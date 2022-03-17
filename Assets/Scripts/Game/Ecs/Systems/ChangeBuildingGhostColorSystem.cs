using Game.Ecs.Components;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems {
    public partial class ChangeBuildingGhostColorSystem : SystemBase {
        private static readonly int EmissionColor = Shader.PropertyToID("_EMISSION_COLOR");

        protected override void OnUpdate() {
            Entities.WithAll<Tag_AvailableForPlacementGhostQuad>().ForEach((ref Parent parent) => {
                SetRenderMeshColor(parent.Value, Color.green);
            }).WithoutBurst().Run();
            
            Entities.WithNone<Tag_AvailableForPlacementGhostQuad>().ForEach((Tag_BuildingGhostPositioningQuad ghostQuad, ref Parent parent) => {
                SetRenderMeshColor(parent.Value, Color.red);
            }).WithoutBurst().Run();
        }

        private void SetRenderMeshColor(Entity ghost, Color color) {
            RenderMesh renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(ghost);
            renderMesh.material.SetColor(EmissionColor, color);
        }
    }
}
