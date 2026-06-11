using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cohesion : Seek {

    [SerializeField]
    private float _thresholdCohesion = 0f;

    [SerializeField]
    // Targets that I want to approach 
    private List<Agent> _targets;

    public List<Agent> Targets {
        get => _targets;
        set => _targets = value;
    }

    public override Steering GetSteering(AgentNPC agent) {

        Vector3 direction;
        Vector3 centerOfMass = Vector3.zero;
        float distance = 0f;
        int count = 0;
        
        // Loop through each target and calculate center of mass
        foreach (Agent target in _targets) {
            direction = agent.Position - target.Position;
            distance = Mathf.Abs(direction.magnitude);

            if (distance < _thresholdCohesion) {
                centerOfMass += target.Position;
                count++;
            }
        }

        if (count == 0) {
            Steering steering;
            steering.linear = Vector3.zero;
            steering.angular = 0f;
            return steering;
        }

        centerOfMass /= count;

        // The objective is the center of mass. The point I want to approach
        Target = centerOfMass;
        
        return base.GetSteering(agent);
    }
}