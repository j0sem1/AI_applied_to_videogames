using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendedSteering : SteeringBehaviour {

    // Holds a list of BehaviourAndWeight instances.
    [SerializeField]
    private List<SteeringBehaviour> _behaviours;

    // Holds the maximum acceleration and rotation
    private float _magnitude;

    public override Steering GetSteering(AgentNPC agent) {
        Steering steering;
        steering.linear = new Vector3 (0,0,0);
        steering.angular = 0;

        // Accumulate all accelerations.
        foreach (var b in _behaviours) {
            Steering agentSteering = b.GetSteering(agent);
            steering.linear += b.Weight * agentSteering.linear;
            steering.angular += b.Weight * agentSteering.angular;
        }

        // Crop the result and return.
        _magnitude = Mathf.Min(steering.linear.magnitude, agent.MaxAcceleration);
        steering.linear.Normalize();
        steering.linear = steering.linear * _magnitude;
        steering.angular = Mathf.Min(steering.angular, agent.MaxAngularAcceleration);

        return steering;
    }
    
    // Not implemented
    public override KinematicSteering GetKinematicSteering(AgentNPC agent) {
        KinematicSteering steering;
        steering.velocity = new Vector3 (0,0,0);
        steering.rotation = 0;

        return steering;
    }
}