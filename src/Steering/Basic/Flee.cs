using UnityEngine;

public class Flee : SteeringBehaviour {

    [SerializeField]
    private float _threshold;
   
    [SerializeField]
    private Vector3 _target;

    public float Threshold {
        get => _threshold;
        set => _threshold = value;
    }

    public Vector3 Target {
        get => _target;
        set => _target = value;
    }

    [SerializeField]
    private bool _drawTargetGizmo;

    public override Steering GetSteering(AgentNPC agent) {
        Steering steering;
        // Get the direction to the target
        steering.linear = agent.Position - Target;

        // Account for rounding 
        if (steering.linear.magnitude > Threshold) {
            steering.linear = -agent.Velocity;
        }
        // Give full acceleration along this direction
        steering.linear.Normalize();
        steering.linear *= agent.MaxAcceleration;
    
        // Output the steering
        steering.angular = 0;
        return steering;
    }
    
    // Implemented
    public override KinematicSteering GetKinematicSteering(AgentNPC agent) {
        KinematicSteering steering;
        
        // Get the direction to the target
        var direction = agent.Position - Target;
        steering.velocity = direction;
        
        // Give full velocity along this direction
        steering.velocity.Normalize();
        steering.velocity *= agent.MaxVelocity;
        
        // Check if Flee agent is too far from target
        if (direction.magnitude > Threshold) {
            // If so, return no steering
            steering.velocity = Vector3.zero;
        }
        // Face in the direction we want to move
        agent.Orientation = VectorToOrientation(agent.Orientation, steering.velocity);
        
        steering.rotation = 0;
        return steering;
    }

    private void OnDrawGizmos() {
        if (_drawTargetGizmo) {
            Gizmos.color = _targetColor;
            Gizmos.DrawSphere(Target, 0.3f);
        }
    }
}
