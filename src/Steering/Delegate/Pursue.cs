using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649 // Disable "variable not initialized" warnings due to serializefield
public class Pursue : Seek {

    [SerializeField]
    private float _maxPrediction;
    public Agent Pursued;

    public override Steering GetSteering(AgentNPC agent) {

        // Work out the distance to target
        Vector3 direction = Pursued.Position - agent.Position;
        float distance = direction.magnitude;

        // Work out our current speed
        float speed = agent.Velocity.magnitude;

        // Check if speed is too small to give a reasonable
        // prediction time
        
        float prediction;
        
        if (speed <= (distance / _maxPrediction)) {
            prediction = _maxPrediction;
        }
         // Otherwise calculate the prediction time
        else {
            prediction = distance / speed;
        }

        // Put the target together
        this.Target = Pursued.Position;
        this.Target += Pursued.Velocity * prediction;
        
        // Delegate to seek
        return base.GetSteering(agent);
        
    }
}
