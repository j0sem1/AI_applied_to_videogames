using UnityEngine;

public class Arrive : SteeringBehaviour {

    [SerializeField]
    private float _timeToTarget = 0.1f;

    [SerializeField]
    private Vector3 _target;

    public Vector3 Target {
        get => _target;
        set => _target = value;
    }
    
    [SerializeField]
    private bool _drawTargetGizmo;

    void Start() {
        _target = transform.position;
    }

    public override Steering GetSteering(AgentNPC agent) {
        Steering steering;
        steering.angular = 0;
        // Get the direction to the target
        Vector3 direction = Target - agent.Position;
        float distance = direction.magnitude;

        float targetSpeed;
        // Check if we are there, return no steering in that case
        if (distance <= agent.InteriorRadius) {
            // Return "none"
            steering.linear = -agent.Velocity;
            if (steering.linear.magnitude > 0) {
                steering.linear.Normalize();
                steering.linear *= agent.MaxAcceleration;
            }
            return steering;
        }

        // If we are outside the slowRadius, then use maximum speed
        if (distance > agent.ExteriorRadius) {
            targetSpeed = agent.MaxVelocity;
        }
        else {
            // Otherwise calculate a scaled speed
            targetSpeed = agent.MaxVelocity * distance / agent.ExteriorRadius;
        }

        // The target velocity combines speed and direction
        Vector3 targetVelocity;
        targetVelocity = direction;
        targetVelocity.Normalize();
        targetVelocity *= targetSpeed;

        // Acceleration tries to get to the target velocity
        steering.linear = targetVelocity - agent.Velocity;
        steering.linear /= _timeToTarget;

        // Check if the acceleration is too great
        if (steering.linear.magnitude > agent.MaxAcceleration) {
            steering.linear.Normalize();
            steering.linear *= agent.MaxAcceleration;
        }

        // Output the steering
        return steering;
    }
    
    // Not implemented
    public override KinematicSteering GetKinematicSteering(AgentNPC agent) {
        KinematicSteering steering;
        steering.velocity = Vector3.zero;
        steering.rotation = 0;
        return steering;
    }

    private void OnDrawGizmos() {
        if (_drawTargetGizmo) {
            Gizmos.color = _targetColor;
            Gizmos.DrawSphere(_target, 0.3f);
        }
    }
}
