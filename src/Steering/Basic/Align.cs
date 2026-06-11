using UnityEngine;

public class Align : SteeringBehaviour {
    [Header("Align")]
    [SerializeField]
    private float _timeToTarget = 0.1f;

    [SerializeField]
    private float _targetOrientation;
    
    public float TargetOrientation {
        get => _targetOrientation;
        set => _targetOrientation = value;
    }
    public override Steering GetSteering(AgentNPC agent) {

        Steering steering;
        steering.linear = Vector3.zero;
        
        // Get the naive direction to the target
        float rotation = _targetOrientation - agent.Orientation;

        // Map the result to the (-pi, pi) interval
        rotation = -Mathf.PI + Mathf.Repeat(rotation + Mathf.PI, 2*Mathf.PI);

        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering in that case
        if (rotationSize <= agent.InteriorAngle) {

            // Return "none"
            steering.angular = -agent.Rotation;
            if (steering.angular > 0) {
                steering.angular /= Mathf.Abs(steering.angular);
                steering.angular *= agent.MaxAngularAcceleration;
            }
            return steering;
        }

        float targetRotation;
        // If we are outside the slowRadius, then use maximum rotation
        if (rotationSize > agent.ExteriorAngle) {
            targetRotation = agent.MaxRotation;
        }
        else {
            // Otherwise calculate a scaled rotation
            targetRotation = agent.MaxRotation * rotationSize / agent.ExteriorAngle;
        }

        // The final target rotation combines speed (already in the variable) and direction
        targetRotation *= rotation / rotationSize;

        // Acceleration tries to get to the target rotation
        steering.angular = targetRotation - agent.Rotation;
        steering.angular /= _timeToTarget;

        // Check if the acceleration is too great
        if (Mathf.Abs(steering.angular) > agent.MaxAngularAcceleration) {
            steering.angular /= Mathf.Abs(steering.angular);
            steering.angular *= agent.MaxAngularAcceleration;
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
}
