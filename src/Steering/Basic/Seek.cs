using UnityEngine;

public class Seek : SteeringBehaviour {

    [Header("Seek")]
    [SerializeField]
    private float _threshold = 0f;
    public float Threshold => _threshold;

    [SerializeField]
    private Vector3 _target;

    [SerializeField]
    private bool _drawTargetGizmo;

    [SerializeField] 
    private bool _applyKinematicRotation = true;

    public Vector3 Target {
        get => _target;
        set => _target = value;
    }

    public override Steering GetSteering(AgentNPC agent) {
        Steering steering;
        // Get the direction to the target
        steering.linear = _target - agent.Position;

        // Account for rounding 
        if (steering.linear.magnitude < _threshold) {
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
        var direction = Target - agent.Position;
        steering.velocity = direction;
        
        steering.velocity.Normalize();
        steering.velocity *= agent.MaxVelocity;
        
        // Check if we're going to overshoot by applying the output velocity
        var updatedMovement = steering.velocity * Time.deltaTime;
        if (direction.magnitude <= _threshold) {
            // If so, don't do it
            steering.velocity = Vector3.zero;
            agent.Position = Target;
        }

        if (_applyKinematicRotation)
            // Face in the direction we want to move
            agent.Orientation = VectorToOrientation(agent.Orientation, steering.velocity);
        
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
