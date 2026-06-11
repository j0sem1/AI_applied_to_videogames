using UnityEngine;

#pragma warning disable 0649 // Disable "variable not initialized" warnings due to serializefield
public class Wander : Face {

    // Holds the radius and forward offset of the wander
    [SerializeField]
    private float _wanderOffset;
    [SerializeField]
    private float _wanderRadius;

    // Holds the maximum rate at which the wander orientation can change
    [SerializeField]
    private float _wanderRate;

    // Holds the current orientation of the wander target
    private float _wanderOrientation;

    // Returns a random number between -1 and 1, where values around zero are more likely.
    private float RandomBinomial() {
        // With Random.Range() min is exclusive and max inclusive.
        return Random.Range(0.0f, 1.0f) - Random.Range(0.0f, 1.0f);
    }

    /*
     * Get a vector form of the orientation.
     * Calculate a unit vector in the direction that the character is facing.
    */
    private Vector3 AsVector(float orientation) {
        return new Vector3(Mathf.Sin(orientation), 0, Mathf.Cos(orientation));
    }

    public override Steering GetSteering(AgentNPC agent) {
        Steering steering;
        steering.linear = Vector3.zero;
        steering.angular = 0f;

        // Update the wander orientation
        _wanderOrientation += RandomBinomial() * _wanderRate;

        // Calculate the combined target orientation
        TargetOrientation = _wanderOrientation + agent.Orientation;

        // Calculate the center of the wander circle
        TargetPosition = agent.Position + _wanderOffset * AsVector(agent.Orientation);

        // Calculate the target location
        TargetPosition += _wanderRadius * AsVector(TargetOrientation);

        // Delegate to Face
        steering = base.GetSteering(agent);

        // Set the linear acceleration to be at full acceleration in the direction of the orientation
        steering.linear = agent.MaxAcceleration * AsVector(agent.Orientation);
        
        // Return it
        return steering;
    }

    public override KinematicSteering GetKinematicSteering(AgentNPC agent) {
        // Create the structure for output
        KinematicSteering steering;

        // Get velocity from the vector form of the orientation
        steering.velocity = agent.MaxVelocity * AsVector(agent.Orientation);

        // Change our orientation randomly
        float random = RandomBinomial();
        steering.rotation = random * agent.MaxRotation;

        // Output the steering
        return steering;
    }

}
