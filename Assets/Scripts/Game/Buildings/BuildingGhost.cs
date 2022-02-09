using Game;
using UnityEngine;
using Utils;

public class BuildingGhost {
    private GameObject prefab;
    private Transform transform;
    private Camera camera;
    private LayerMask layerMask;
    
    public BuildingGhost(GameObject prefab, Camera cam, LayerMask layerMask) {
        this.prefab = prefab;
        this.camera = cam;
        this.layerMask = layerMask;
    }

    public void Start() {
        transform = Object.Instantiate(prefab).transform;
    }

    public void Update() {
        transform.position = BuildingGrid.WorldToGridCentered(InputUtility.MouseToWorld(camera, layerMask));
    }

    public void Dispose() { 
        Object.Destroy(transform.gameObject);
    }
}
