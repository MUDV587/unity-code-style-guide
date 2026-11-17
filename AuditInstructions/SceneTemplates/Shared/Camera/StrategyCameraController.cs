using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityStarter.SceneTemplates
{
    /// <summary>
    /// Standalone strategy camera. Owns a private input instance; the host can block navigation
    /// when UI or tools own input. Distances are world units and angles are degrees.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StrategyCameraController : MonoBehaviour
    {
        private const string MapName = "Camera";
        [Header("Rig")]
        [SerializeField] private Transform _pitchPivot;
        [SerializeField] private Camera _lens;
        [Tooltip("Shared source definition; each rig owns a private runtime copy.")]
        [SerializeField] private InputActionAsset _inputDefinition;
        [Header("Navigation")]
        [SerializeField, Min(0.1f)] private float _panSpeed = 25f;
        [SerializeField, Min(1f)] private float _sprintMultiplier = 3f;
        [SerializeField, Min(0f)] private float _rotationSpeed = 100f;
        [Tooltip("Yaw degrees per pointer pixel while the middle mouse button is held.")]
        [SerializeField, Min(0f)] private float _dragSensitivity = 0.15f;
        [SerializeField, Min(0.01f)] private float _zoomSensitivity = 8f;
        [SerializeField, Min(0f)] private float _smoothingTime = 0.12f;
        [Header("View")]
        [SerializeField, Min(0.1f)] private float _minDistance = 12f;
        [SerializeField, Min(0.1f)] private float _maxDistance = 120f;
        [SerializeField, Min(0.1f)] private float _startDistance = 55f;
        [SerializeField, Range(15f, 80f)] private float _closePitch = 35f;
        [SerializeField, Range(15f, 80f)] private float _farPitch = 65f;
        [SerializeField, Range(15f, 80f)] private float _orthographicPitch = 45f;
        [SerializeField] private bool _isOrthographic;
        private InputActionAsset _inputInstance;
        private InputActionMap _map;
        private InputAction _pan;
        private InputAction _rotate;
        private InputAction _zoom;
        private InputAction _drag;
        private InputAction _look;
        private InputAction _sprint;
        private InputAction _toggleProjection;
        private Vector3 _targetPosition;
        private Vector3 _positionVelocity;
        private float _targetYaw;
        private float _yawVelocity;
        private float _distance;
        private float _targetDistance;
        private float _zoomVelocity;
        private bool _isInputBlocked;
        private bool _hasFocus = true;

        /// <summary>Whether navigation is blocked by the host UI or a construction tool.</summary>
        public bool IsInputBlocked => _isInputBlocked;
        /// <summary>Whether the lens uses the fixed-tilt orthographic view.</summary>
        public bool IsOrthographic => _isOrthographic;
        /// <summary>Whether this rig currently owns a middle-mouse camera drag.</summary>
        public bool IsDragging => !_isInputBlocked && _hasFocus && _drag != null && _drag.IsPressed();

        private void Awake()
        {
            if (_pitchPivot == null || _lens == null || _inputDefinition == null)
            {
                Debug.LogError("Strategy camera requires a pitch pivot, lens, and input definition.", this);
                enabled = false;
                return;
            }

            // Enabling one rig must never enable or disable another rig's shared source actions.
            _inputInstance = Instantiate(_inputDefinition);
            _map = _inputInstance.FindActionMap(MapName, true);
            _pan = _map.FindAction("Pan", true);
            _rotate = _map.FindAction("Rotate", true);
            _zoom = _map.FindAction("Zoom", true);
            _drag = _map.FindAction("Drag", true);
            _look = _map.FindAction("Look", true);
            _sprint = _map.FindAction("Sprint", true);
            _toggleProjection = _map.FindAction("ToggleProjection", true);
            _distance = Mathf.Clamp(_startDistance, _minDistance, _maxDistance);
            _targetDistance = _distance;
            SynchronizePose();
            ApplyLens();
        }

        private void OnEnable()
        {
            SynchronizePose();
            _map?.Enable();
        }

        private void OnDisable()
        {
            _map?.Disable();
            SynchronizePose();
        }

        private void OnDestroy()
        {
            if (_inputInstance != null)
            {
                _inputInstance.Disable();
                Destroy(_inputInstance);
            }
        }

        private void Update()
        {
            if (_map == null)
            {
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;
            if (!_isInputBlocked && _hasFocus)
            {
                ReadNavigation(deltaTime);
            }

            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition,
                ref _positionVelocity, _smoothingTime, Mathf.Infinity, deltaTime);
            float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetYaw,
                ref _yawVelocity, _smoothingTime, Mathf.Infinity, deltaTime);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            _distance = Mathf.SmoothDamp(_distance, _targetDistance,
                ref _zoomVelocity, _smoothingTime, Mathf.Infinity, deltaTime);
            ApplyLens();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _hasFocus = hasFocus;
            if (!hasFocus)
            {
                SynchronizePose();
            }
        }

        private void OnValidate()
        {
            _minDistance = Mathf.Max(0.1f, _minDistance);
            _maxDistance = Mathf.Max(_minDistance, _maxDistance);
            _startDistance = Mathf.Clamp(_startDistance, _minDistance, _maxDistance);
        }

        /// <summary>Blocks new navigation and cancels pending camera movement when true.</summary>
        public void SetInputBlocked(bool isBlocked)
        {
            _isInputBlocked = isBlocked;
            if (isBlocked)
            {
                SynchronizePose();
            }
        }

        /// <summary>Changes projection while keeping the rig's ground focus and current zoom scale.</summary>
        public void SetOrthographic(bool isOrthographic)
        {
            _isOrthographic = isOrthographic;
            ApplyLens();
        }

        private void ReadNavigation(float deltaTime)
        {
            float sprint = _sprint.IsPressed() ? _sprintMultiplier : 1f;
            Vector2 pan = Vector2.ClampMagnitude(_pan.ReadValue<Vector2>(), 1f);
            float zoomScale = Mathf.Max(1f, _distance / _startDistance);
            _targetPosition += transform.TransformDirection(new Vector3(pan.x, 0f, pan.y))
                * (_panSpeed * zoomScale * sprint * deltaTime);
            _targetYaw += _rotate.ReadValue<float>() * _rotationSpeed * deltaTime;
            if (IsDragging)
            {
                _targetYaw += _look.ReadValue<Vector2>().x * _dragSensitivity;
            }

            // Wheel events are deltas, so they must not be multiplied by frame duration.
            float scroll = Mathf.Clamp(_zoom.ReadValue<float>(), -1f, 1f);
            _targetDistance = Mathf.Clamp(_targetDistance - scroll * _zoomSensitivity * zoomScale * sprint,
                _minDistance, _maxDistance);
            if (_toggleProjection.WasPressedThisFrame())
            {
                SetOrthographic(!_isOrthographic);
            }
        }

        private void SynchronizePose()
        {
            _targetPosition = transform.position;
            _targetYaw = transform.eulerAngles.y;
            _targetDistance = _distance;
            _positionVelocity = Vector3.zero;
            _yawVelocity = 0f;
            _zoomVelocity = 0f;
        }

        private void ApplyLens()
        {
            if (_lens == null || _pitchPivot == null)
            {
                return;
            }

            float zoom = Mathf.InverseLerp(_minDistance, _maxDistance, _distance);
            float pitch = _isOrthographic ? _orthographicPitch : Mathf.Lerp(_closePitch, _farPitch, zoom);
            _pitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            _lens.transform.localPosition = new Vector3(0f, 0f, -_distance);
            _lens.orthographic = _isOrthographic;
            _lens.orthographicSize = _distance * Mathf.Tan(_lens.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }
    }
}
