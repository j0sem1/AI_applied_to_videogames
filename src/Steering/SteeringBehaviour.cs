using UnityEngine;

public abstract class SteeringBehaviour : MonoBehaviour {

    [System.Serializable]
    public struct Steering {
        public Vector3 linear;
        public float angular;
    }

    [System.Serializable]
    public struct KinematicSteering {
        public Vector3 velocity;
        public float rotation;
    }

    [SerializeField]
    private float _weight;
    
    public float Weight {
        get => _weight;
        set => _weight = value;
    }

    // White
    protected Color _targetColor = new Color(1f, 1f, 1f);

    public abstract Steering GetSteering(AgentNPC agent);

    // Not mandatory to implement
    public abstract KinematicSteering GetKinematicSteering(AgentNPC agent);

    protected float VectorToOrientation(float orientation, Vector3 velocity) {
        if (velocity.magnitude > 0) {
            return Mathf.Atan2(velocity.x, velocity.z);
        }
        return orientation;
    }
}
