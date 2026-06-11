using UnityEngine;

#pragma warning disable 0649 // Disable "variable not initialized" warnings due to serializefield
public class ObstacleAvoidance : Seek {

    [Header("Obstacle Avoidance")]
    [SerializeField]
    private bool _drawGizmos;

    [SerializeField]
    private float _avoidDistance;

    [SerializeField]
    private float _primaryLookahead;

    [SerializeField]
    private float _whiskersLookahead;
    
    [SerializeField]
    private float _whiskersAngle = 15f;
    
    public override Steering GetSteering(AgentNPC agent) {
        // Calculate the collision ray vector
        Vector3 primaryVector = agent.Velocity.normalized;
        Vector3 leftWhiskerVector = Quaternion.Euler(0, -_whiskersAngle, 0) * primaryVector;
        Vector3 rightWhiskerVector = Quaternion.Euler(0, _whiskersAngle, 0) * primaryVector;

        // Find the collision (priority: frontal > left > right)
        RaycastHit primaryHit, leftWhiskerHit, rightWhiskerHit;

        if (Physics.Raycast(agent.Position, primaryVector, out primaryHit, _primaryLookahead)) {
            // We detected a frontal collision
            Target = primaryHit.point + primaryHit.normal * _avoidDistance;
            return base.GetSteering(agent);
        } 

        // No frontal collision, check the left whisker
        if (Physics.Raycast(agent.Position, leftWhiskerVector, out leftWhiskerHit, _primaryLookahead)) {
            // We detected a left side collision
            Target = leftWhiskerHit.point + leftWhiskerHit.normal * _avoidDistance;
            return base.GetSteering(agent);
        } 

        // No left side collision, check the right whisker
        if (Physics.Raycast(agent.Position, rightWhiskerVector, out rightWhiskerHit, _primaryLookahead)) {
            // We detected a right side collision
            Target = rightWhiskerHit.point + rightWhiskerHit.normal * _avoidDistance;
            return base.GetSteering(agent);
        } 

        // No collision detected, do nothing
        Steering steering;
        steering.linear = Vector3.zero;
        steering.angular = 0f;
        return steering;
    }

    private Color avoidDistanceColor = new Color(0.96f, 0.73f, 0.2f);
    private Color primaryRayColor = new Color(0.74f, 0.31f, 0.31f);
    private Color whiskerColor = new Color(0.31f, 0.74f, 0.31f);
    private void OnDrawGizmos() {
        if (_drawGizmos) {
            Gizmos.color = avoidDistanceColor;
            Gizmos.DrawWireSphere(transform.position, _avoidDistance);

            Vector3 primaryVector = transform.forward;
            Vector3 leftWhiskerVector = Quaternion.Euler(0, -_whiskersAngle, 0) * primaryVector;
            Vector3 rightWhiskerVector = Quaternion.Euler(0, _whiskersAngle, 0) * primaryVector;

            Vector3 position = transform.position;

            Gizmos.color = primaryRayColor;
            Gizmos.DrawLine(position, position + primaryVector * _primaryLookahead);

            Gizmos.color = whiskerColor;
            Gizmos.DrawLine(position, position + leftWhiskerVector * _whiskersLookahead);
            Gizmos.DrawLine(position, position + rightWhiskerVector * _whiskersLookahead);
        }
    }
}
