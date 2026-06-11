using UnityEngine;

public class LookWhereYoureGoing : Align {
    
    [Header("LookWhereYoureGoing")]
    // Alternative, used for the strategy scene
    [SerializeField]
    private AgentNPC _externalTarget;
    
    public override Steering GetSteering(AgentNPC agent) {
        if (_externalTarget != null) {
            if (_externalTarget.Velocity.magnitude < 0.05) {
                // Return "none"
                TargetOrientation = agent.Orientation;
            } else {
                TargetOrientation = Mathf.Atan2(_externalTarget.Velocity.x, _externalTarget.Velocity.z);
            }
            return base.GetSteering(agent);
        }

        if (agent.Velocity.magnitude < 0.05) {
            // Return "none"
            TargetOrientation = agent.Orientation;
        } else {
            TargetOrientation = Mathf.Atan2(agent.Velocity.x, agent.Velocity.z);
        }
        return base.GetSteering(agent);
    }
}
