using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alignment : Align {
    [SerializeField]
    private float _threshold = 0f;

    [SerializeField]
    // Targets that have see to the same direction
    private List<Agent> _targets;

    public List<Agent> Targets {
        get => _targets;
        set => _targets = value;
    }

    public override Steering GetSteering(AgentNPC agent) {
        Vector3 direction;
        float heading = 0f;
        float distance = 0f;
        int count = 0;

        // Loop through each target. Get the medium point to all targets have to watch
        foreach (Agent target in _targets) {
            direction = agent.Position - target.Position;
            distance = Mathf.Abs(direction.magnitude);

            if (distance < _threshold) {
                heading += target.Orientation;
                count++;
            }
        }

        if (count > 0) {
            heading /= count;
        }

        TargetOrientation = heading;

        return base.GetSteering(agent);
    }
    
    // Not implemented
    public override KinematicSteering GetKinematicSteering(AgentNPC agent) {
        KinematicSteering steering;
        steering.velocity = Vector3.zero;
        steering.rotation = 0;
        return steering;
    }
}