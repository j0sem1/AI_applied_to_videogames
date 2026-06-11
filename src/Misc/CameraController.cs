using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] private float _movementVelocity = 0.25f;
    [SerializeField] private float _rotationVelocity = 0.1f;

    [SerializeField] private float _zoomVelocity = 0.1f;
    [SerializeField] private float _maxCameraZoom = 4;
    [SerializeField] private float _minCameraZoom = 15;
    [SerializeField] private float _currentZoom = 7;
    [SerializeField] private bool _allowRotation = true;
    private Camera _camera;
    private Transform _cameraTarget; // Parent object responsible for the isometric perspective
    private float _horizontalMovement;
    private float _verticalMovement;
    private float _wheelMovement;
    private float _currentY;
    private float _targetY;
    private float _currentVelocity;
    
    private float _targetZoom;
    private float _currentZoomVelocity;

    void Start() {
        _targetZoom = _currentZoom;
        _cameraTarget = transform.parent;
        _currentY = transform.parent.localEulerAngles.y;
        _targetY = _currentY;
        _camera = GetComponent<Camera>();
        _camera.orthographicSize = _currentZoom;
    }

    // Update is called once per frame
    void Update() {

        _horizontalMovement = Input.GetAxis("Horizontal");
        _verticalMovement = Input.GetAxis("Vertical");
        _wheelMovement = Input.GetAxis("Mouse ScrollWheel");

        // Simple horizontal and vertical movement
        if (_horizontalMovement != 0 || _verticalMovement != 0) {
            Vector3 position = transform.localPosition;

            position.x += _horizontalMovement * _movementVelocity;
            position.y += _verticalMovement *_movementVelocity;
            transform.localPosition = position;
        }
        
        // Rotation
        if (_allowRotation) {
            // Rotate 90º clockwise
            if (Input.GetKeyDown(KeyCode.Q)) {
                _targetY = (_targetY - 90);
            }

            // Rotate 90º counter clockwise
            if (Input.GetKeyDown(KeyCode.E)) {
                _targetY = (_targetY + 90);
            }

            if (_currentY != _targetY) {
                _currentY = Mathf.SmoothDamp(_currentY, _targetY, ref _currentVelocity, _rotationVelocity);
                Vector3 angle = _cameraTarget.transform.localEulerAngles;
                angle.y = _currentY;
                _cameraTarget.transform.localEulerAngles = angle;
            }
        }

        // Zooming
        if (_wheelMovement != 0) {
            _targetZoom += _wheelMovement * -8;
            _targetZoom = Mathf.Clamp(_targetZoom, _maxCameraZoom, _minCameraZoom);
        }

        if (_currentZoom != _targetZoom) {
            _currentZoom = Mathf.SmoothDamp(_currentZoom, _targetZoom, ref _currentZoomVelocity,_zoomVelocity);
            _camera.orthographicSize = _currentZoom;
        }
    }
}
