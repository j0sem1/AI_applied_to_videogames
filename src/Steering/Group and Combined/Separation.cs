using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Separation : SteeringBehaviour {
    [SerializeField]
    private float _threshold = 0f;

    [SerializeField]
    // Targets I want to get away from
    private List<Agent> _targets;

    public List<Agent> Targets {
        get => _targets;
        set => _targets = value;
    }

    // Hold the constant coefficient of decay for 
    // the inverse square law force
    [SerializeField]
    private float _decayCoefficient;

    [SerializeField]
    private float _strength;

    public override Steering GetSteering(AgentNPC agent) {

        Steering steering;
        steering.linear = Vector3.zero;
        steering.angular = 0f;

        Vector3 direction;

        float distance = 0f;

        // Loop through each target
        foreach (Agent target in _targets) {

            // Check if the target is close
            direction = agent.Position - target.Position;
            distance = Mathf.Abs(direction.magnitude);

            if (distance < _threshold) {

                // Calculate the strength of repulsion
                float desiredStrength = _decayCoefficient/(distance*distance);
                _strength = Mathf.Min(desiredStrength, agent.MaxAcceleration);

                // Add the acceleration
                steering.linear += direction.normalized * _strength;
            }
        }

        // We've gone through all targets, return the result
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