using UnityEngine;
#pragma warning disable 0649 // Disable "variable not initialized" warnings due to serializefield
public class Agent : MonoBehaviour {

    [Header("Physical attributes")]
    // Physics related information
    [SerializeField]
    private float _orientation;
    [SerializeField]
    private float _rotation;
    [SerializeField]
    private Vector3 _velocity;
    [SerializeField]
    private float _maxVelocity;
    [SerializeField]
    private float _maxAcceleration;
    [SerializeField]
    private float _maxRotation;
    [SerializeField]
    private float _maxAngular;
    
    [Header("Agent information")]
    [SerializeField]
    private float _interiorRadius;
    [SerializeField]
    private float _exteriorRadius;
    [SerializeField]
    private float _interiorAngle;
    [SerializeField]
    private float _exteriorAngle;
    [SerializeField]
    private bool _drawRadius;
    [SerializeField]
    private bool _drawAngles;
    [SerializeField]
    private bool _drawVelocity;
    [SerializeField]
    protected bool _drawAcceleration;
    // Yellow
    private Color _interiorRadiusColor = new Color(0.96f, 0.73f, 0.2f);
    // Blue
    private Color _exteriorRadiusColor = new Color(0.2f, 0.71f, 0.96f);
    // Red
    private Color _interiorAngleColor = new Color(0.74f, 0.31f, 0.31f);
    // Green
    private Color _exteriorAngleColor = new Color(0.31f, 0.74f, 0.31f);
    // Orange
    private Color _velocityColor = new Color(0f, 0.498f, 1f);

    private Quaternion _default = new Quaternion();

    public Vector3 Position {
        get => transform.position;
        set => transform.position = value;
    }

    public float Orientation {
        get => _orientation;
        set {
            _orientation = value;

            _orientation = -Mathf.PI + Mathf.Repeat(_orientation + Mathf.PI, 2*Mathf.PI);
            transform.rotation = _default;
            transform.Rotate(Vector3.up, _orientation*Mathf.Rad2Deg);
        }
    }

    public Vector3 Velocity {
        get {
            if (_velocity.magnitude > _maxVelocity) {
                _velocity = _velocity.normalized * _maxVelocity; 
            }
                
            if (_velocity.magnitude < 0.15) 
                _velocity = Vector3.zero;
            return _velocity;
        }
        set {
            _velocity = value;
            if (_velocity.magnitude > _maxVelocity) {
                _velocity = _velocity.normalized * _maxVelocity; 
            }
            if (_velocity.magnitude < 0.1)
                _velocity = Vector3.zero;
        }
    }

    public float Rotation {
        get {
            if (Mathf.Abs(_rotation) < 0.1) {
                _rotation = 0f;
            }
            return _rotation;
        }
        set {
            if (Mathf.Abs(_rotation) < 0.1) {
                _rotation = 0f;
            }
            _rotation = value;
        }
    }

    public float MaxVelocity {
        get => _maxVelocity;
        set => _maxVelocity = value;
    }

    public float MaxRotation => _maxRotation;

    public float MaxAcceleration => _maxAcceleration;

    public float InteriorRadius {
        get {
            if (_interiorRadius < 0) 
                _interiorRadius = 0; 
            if (_interiorRadius > _exteriorRadius)
                _exteriorRadius = _interiorRadius;    
            return _interiorRadius;
        }
    }

    public float ExteriorRadius {
        get {
            if (_exteriorRadius < 0) 
                _exteriorRadius = 0; 
            if (_interiorRadius > _exteriorRadius)
                _exteriorRadius = _interiorRadius;
            return _exteriorRadius;
        }
    }

    public float InteriorAngle {
        get {
            if (_interiorAngle < 0) 
                _interiorAngle = 0; 
            if (_interiorAngle > _exteriorAngle)
                _exteriorAngle = _interiorAngle;                
            return _interiorAngle;
        }
    }

    public float ExteriorAngle {
        get {
            if (_exteriorAngle < 0) 
                _exteriorAngle = 0; 
            if (_interiorAngle > _exteriorAngle)
                _exteriorAngle = _interiorAngle;         
            return _exteriorAngle;
        }
    }

    public float MaxAngularAcceleration => _maxAngular;

    void Start() {
        _velocity = Vector3.zero;
        _rotation = 0;
    }

    protected virtual void OnDrawGizmos() {
        if (_drawRadius) {
            Gizmos.color = _interiorRadiusColor;
            // Draw interior radius
            Gizmos.DrawWireSphere(Position, InteriorRadius);

            Gizmos.color = _exteriorRadiusColor;
            // Draw exterior radius
            Gizmos.DrawWireSphere(Position, ExteriorRadius);
        }
        if (_drawAngles) {
            Quaternion intRot1 = Quaternion.Euler(0, InteriorAngle*Mathf.Rad2Deg, 0);
            Quaternion intRot2 = Quaternion.Euler(0, -InteriorAngle*Mathf.Rad2Deg, 0);
            Gizmos.color = _interiorAngleColor;
            var position = transform.position;
            var forward = transform.forward;
            Gizmos.DrawLine(position, position + intRot1 * forward);
            Gizmos.DrawLine(position, position + intRot2 * forward);

            Quaternion extRot1 = Quaternion.Euler(0, ExteriorAngle*Mathf.Rad2Deg, 0);
            Quaternion extRot2 = Quaternion.Euler(0, -ExteriorAngle*Mathf.Rad2Deg, 0);
            Gizmos.color = _exteriorAngleColor;
            Gizmos.DrawLine(position, position + extRot1 * forward);
            Gizmos.DrawLine(position, position + extRot2 * forward);
        }
        if (_drawVelocity) {
            var position = transform.position;
            Gizmos.color = _velocityColor;
            Quaternion orient = Quaternion.Euler(0, _rotation * Mathf.Rad2Deg, 0);
            Gizmos.DrawLine(position, position + orient * _velocity);
        }
    }
}
