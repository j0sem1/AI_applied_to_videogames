using System.Collections.Generic;
using Steering.Delegate;
using UnityEngine;


// Script with the sole purpose of showcasing and debugging the behaviour of various steerings
// specifically those that have the particular target elements they need (Vector3, float, etc) instead of an Agent object
public class DebugManager : MonoBehaviour {

    public Align Align;
    public Agent AlignTarget;

    public AntiAlign AntiAlign;
    public Agent AntiAlignTarget;

    public Seek Seek;

    public Flee Flee;
    public Agent FleeTarget;
    
    public Path Path;
    public List<Transform> PathPoints;

    public FormationManager Formation1;
    public FormationManager Formation2;
    
    private void Start() {
        if (Seek)
            Seek.Target = Seek.transform.position;
        if (Path) {
            foreach (var point in PathPoints) {
                Path.AppendPointToPath(point.position);
            }
        }
            
    }
    
    void Update() {
        if (Align)
            Align.TargetOrientation = AlignTarget.Orientation;
        if (AntiAlign)
            AntiAlign.TargetOrientation = AntiAlignTarget.Orientation;
        if (Flee)
            Flee.Target = FleeTarget.Position;
        
        if (Input.GetKey("z")) {
            if (Formation1 != null) {
                Formation1.UpdateSlots();
            }

            if (Formation2 != null) {
                Formation2.UpdateSlots();
            }
        }
    }
}
