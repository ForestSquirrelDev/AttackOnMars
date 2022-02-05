using UnityEngine;
using Utils.Maths;

namespace Game.CameraManagement {
    public class RTSCameraRig : MonoBehaviour {
        [SerializeField] private float moveSpeed = 0.1f;
        [SerializeField] private float zoomSpeed = 10.0f;
        [SerializeField] private float rotationSpeed = 1f;
        [SerializeField] private float shiftSpeedMultiplyer = 3f;
        [SerializeField] private AnimationCurve heightModifierCurve;
        [SerializeField] private new Camera camera;
        [SerializeField] private LayerMask layerMask;

        private const float min_terrain_height = 30f;
        private const float max_heght = 150f;
        float terrainHeightUnderCamera = 0;

        private bool rotating;

        private Vector2 initialMousePos, currentMousePos;
        private Transform thisTransform;

        private void Awake() {
            thisTransform = transform;
        }

        private void Update() {
            CalculateSpeed(out var horizontalSpeed, out var verticalSpeed, out var scrollSpeed);
            Vector3 combinedMovement = ConstructMovementVectors(scrollSpeed, horizontalSpeed, verticalSpeed);
            thisTransform.position += combinedMovement;
            ClampHeight();
            RotateCamera();
        }
        
        private void CalculateSpeed(out float horizontalSpeed, out float verticalSpeed, out float scrollSpeed) {
            float heightRelativeToTerrain = thisTransform.position.y - min_terrain_height;
            float heightModifier = heightModifierCurve.Evaluate(heightRelativeToTerrain.ClampPos1ToMaxValue() / ( max_heght - min_terrain_height ));
            horizontalSpeed = heightModifier * moveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
            verticalSpeed = heightModifier * moveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
            scrollSpeed = -zoomSpeed * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftShift)) {
                horizontalSpeed *= shiftSpeedMultiplyer;
                verticalSpeed *= shiftSpeedMultiplyer;
                scrollSpeed *= shiftSpeedMultiplyer;
            }
        }
        
        private Vector3 ConstructMovementVectors(float scrollSpeed, float horizontalSpeed, float verticalSpeed) {
            Vector3 vertical = new Vector3(0, scrollSpeed, 0);
            Vector3 lateral = horizontalSpeed * thisTransform.right;
            Vector3 forward = thisTransform.forward;
            forward.y = 0;
            forward.Normalize();
            forward *= verticalSpeed;
            Vector3 combinedMovement = vertical + lateral + forward;
            return combinedMovement;
        }
        
        private void ClampHeight() {
            if (Physics.Raycast(thisTransform.position, Vector3.down, out RaycastHit hit, float.MaxValue, layerMask)) {
                terrainHeightUnderCamera = hit.point.y + 5f;
            }
            float yClamped = Mathf.Clamp(thisTransform.position.y, terrainHeightUnderCamera, max_heght);
            thisTransform.position = new Vector3(thisTransform.position.x, yClamped, thisTransform.position.z);
        }
        
        private void RotateCamera() {
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) {
                SetInitialMousePos();
            }
            bool rightMouseClicked = Input.GetMouseButton(1);
            bool eClicked = Input.GetKey(KeyCode.E);
            bool anyClicked = rightMouseClicked || eClicked;
            if (anyClicked) {
                currentMousePos = Input.mousePosition;
                if (!rotating) initialMousePos = currentMousePos;
                rotating = true;
            } else {
                rotating = false;
            }
            if (rightMouseClicked) {
                float deltaX = (currentMousePos - initialMousePos).y * rotationSpeed * Time.deltaTime;
                float deltaY = (currentMousePos - initialMousePos).x * rotationSpeed * Time.deltaTime;
                thisTransform.rotation *= Quaternion.Euler(new Vector3(0, deltaY, 0));
                camera.transform.rotation *= Quaternion.Euler(new Vector3(-deltaX, 0, 0));
            }
            if (eClicked) {
                float deltaY = (currentMousePos - initialMousePos).x * rotationSpeed * Time.deltaTime;
                thisTransform.rotation *= Quaternion.Euler(new Vector3(0, deltaY, 0));
            }
            initialMousePos = currentMousePos;
        }

        private void SetInitialMousePos() {
            initialMousePos = Input.mousePosition;
        }
    }
}