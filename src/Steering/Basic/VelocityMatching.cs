using UnityEngine;

public class VelocityMatching : SteeringBehaviour {

    [SerializeField]
    private float _timeToTarget = 0.1f;

    public Agent Target;

    [SerializeField]
    private bool _drawTarget;

    public override Steering GetSteering(AgentNPC agent) {

        Steering steering;
        steering.angular = 0;
        
        // Acceleration tries to get to the target velocity
        steering.linear = Target.Velocity - agent.Velocity;
        steering.linear /= _timeToTarget;

        // Check if the acceleration is too fast
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
        if (_drawTarget) {
            Gizmos.color = _targetColor;
            Gizmos.DrawSphere(Target.Position, 0.3f);
        }
    }
}
