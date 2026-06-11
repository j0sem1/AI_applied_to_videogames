using UnityEngine;

public class Face : Align {

    [Header("Face")]
    [SerializeField]
    private AgentNPC _target = null;

    private Vector3 _targetPosition;

    public Vector3 TargetPosition {
        get {
            return _targetPosition;
        }
        set {
            _targetPosition = value;
        }
    }

    public override Steering GetSteering(AgentNPC agent) {
        if (_target != null)
            _targetPosition = _target.Position;

        Steering steering;
        steering.linear = Vector3.zero;
        steering.angular = 0f;

        // Work out the direction to target
        Vector3 direction = _targetPosition - agent.Position;

        // Check for a zero direction, and make no change if so
        if (direction.magnitude == 0) {
            return steering;
        }

        TargetOrientation = Mathf.Atan2(direction.x, direction.z);
        // Delegate to align
        return base.GetSteering(agent);
    }
}
