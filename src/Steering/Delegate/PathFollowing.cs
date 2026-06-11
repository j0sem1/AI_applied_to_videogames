using Steering.Delegate;
using UnityEngine;

#pragma warning disable 0649 // Disable "variable not initialized" warnings due to serializefield
public class PathFollowing : Seek {

    [Header("Path Following")] 
    
    // Should the steering patrol the path by going back and forwards continuously? 
    [SerializeField]
    private bool _patrol;
    public bool Patrol {
        get => _patrol;
        set {
            _patrol = value;
            _pathOffset = Mathf.Abs(_pathOffset);
        }
    }

    // Holds the path to follow
    [SerializeField]
    protected Path _path;

    // Holds the distance along the path to generate the target. Can be negative
    // if the character is to move along the reverse direction
    [SerializeField]
    private int _pathOffset;
    
    [SerializeField]
    // Current point of the path
    protected int _currentPos;
    
    // Holds the current position on the path
    protected int _currentParam;   //-> Del mismo tipo que la función getParam

    // Point target of the path
    private int _targetParam;
    
    // Start is called before the first frame update
    private void Start() {
        _currentPos = 0;
    }

    public void ClearPath() {
        _path.ClearPath();
    }

    /*
     * Returns true if the unit has arrived at the end of the path
     */
    public bool EndOfThePath() {
        return _path.Length() - 1 == _currentPos;
    }

    public override Steering GetSteering(AgentNPC agent){

        // Find the current position on the path
        _currentParam = _path.GetParam(agent.Position, _currentPos);

        // The current point of the character is set
        _currentPos = _currentParam;

        if (_patrol) {
            if (_currentPos == _path.Length() - 1)
                _pathOffset = Mathf.Abs(_pathOffset) * -1;
            else if (_currentPos == 0)
                _pathOffset = Mathf.Abs(_pathOffset);
        }

        // Offset it
        _targetParam = _currentParam + _pathOffset;

        // Get the target position
        base.Target = _path.GetPosition(_targetParam);

        return base.GetSteering(agent);
    }

    public override KinematicSteering GetKinematicSteering(AgentNPC agent) {
        // Find the current position on the path
        _currentParam = _path.GetParam(agent.Position, _currentPos);

        // The current point of the character is set
        _currentPos = _currentParam;
        
        if (_patrol) {
            if (_currentPos == _path.Length() - 1)
                _pathOffset = Mathf.Abs(_pathOffset) * -1;
            else if (_currentPos == 0)
                _pathOffset = Mathf.Abs(_pathOffset);
        }


        // Offset it
        _targetParam = _currentParam + _pathOffset;

        // Get the target position
        base.Target = _path.GetPosition(_targetParam);

        return base.GetKinematicSteering(agent);
    }
}
