using System.Collections.Generic;
using UnityEngine;
#pragma warning disable 0649 // Disable "variable not initialized" warnings due to serializefield
public class AgentNPC : Agent {

    [Header("Behaviour")]
    [SerializeField]
    public List<SteeringBehaviour> _steerings;

    [SerializeField] private bool _kinematic = false;

    private Color _accelerationColor;
    private Vector3 _acceleration;

    void Start() {
        _accelerationColor = new Color(1f, 0.1f, 1f); // Pink
        _acceleration = Vector3.zero;
    }

    // Update is called once per frame
    void FixedUpdate() {
        foreach (SteeringBehaviour steering in _steerings) {
            if (_kinematic) 
                ApplyKinematicSteering(steering.GetKinematicSteering(this));
            else
                ApplySteering(steering.GetSteering(this));
        }
    }

    private void ApplySteering(SteeringBehaviour.Steering steering) {
        Position += Velocity * Time.deltaTime;
        Orientation += Rotation * Time.deltaTime;
        Velocity += steering.linear * Time.deltaTime;
        Rotation += steering.angular * Time.deltaTime;
        _acceleration = steering.linear;
    }

    private void ApplyKinematicSteering(SteeringBehaviour.KinematicSteering steering) {
        Position += Velocity * Time.deltaTime;
        Orientation += Rotation * Time.deltaTime;
        Velocity = steering.velocity;
        Rotation += steering.rotation;
    }
    
    protected override void OnDrawGizmos() {
        base.OnDrawGizmos();
        if (_drawAcceleration && !_kinematic) {
            Gizmos.color = _accelerationColor;
            Quaternion orientation = Quaternion.Euler(0, Rotation * Mathf.Rad2Deg, 0);
            Gizmos.DrawLine(transform.position, transform.position + orientation * _acceleration);
        }
    }
}
