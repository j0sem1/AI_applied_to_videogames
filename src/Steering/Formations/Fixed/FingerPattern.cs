using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerPattern : FormationManager {
    // The radius of one character, this is needed to determine how
    // close we can pack a given number of characters around a circle
    [SerializeField]
    private float _characterRadius;

    private float _numberSlots = 4;

    // Send new target locations to each character
    public override void UpdateSlots() {
        // Find the anchor point
        Location leader = GetLeader();
        // For a reason past the developer's comprehension, inverting twice the orientation makes it work
        leader.orientation *= -1; 
        
        int slotAssignmentsSize = _slotAssignments.Count;
        for (int i = 0; i < slotAssignmentsSize; i++) {
            int slotNumber = _slotAssignments[i].SlotNumber;
            Location slot = GetSlotLocation(slotNumber);

            // Transform by the anchor point position and orientation.
            Location location;
            var result = new Vector3(Mathf.Cos(leader.orientation) * slot.position.x -  Mathf.Sin(leader.orientation) * slot.position.z,
                0,
                Mathf.Sin(leader.orientation) * slot.position.x + Mathf.Cos(leader.orientation) * slot.position.z);
            location.position = leader.position + result;
            location.orientation = -(leader.orientation + slot.orientation);

            // Set the position with seek
            _slotAssignments[i].Character.GetComponent<Seek>().Target = location.position;
            // Set the orientation
            _slotAssignments[i].Character.GetComponent<Align>().TargetOrientation = location.orientation;
        }
    }

    // Calculate the position of a slot
    public override Location GetSlotLocation(int slotNumber) {
        Location result;

        if (slotNumber == 0) {
            result.position = new Vector3(0, 0, 0);
            result.orientation = Mathf.PI/2;
        }
        else if (slotNumber == 1) {
            result.position = new Vector3(_characterRadius, 0, _characterRadius);
            result.orientation = 0;
        }
        else if (slotNumber == 2) {
            result.position = new Vector3(_characterRadius * 2, 0, 0);
            result.orientation = - Mathf.PI/4;
        }
        else {
            result.position = new Vector3(_characterRadius * 3, 0, -_characterRadius);
            result.orientation = - Mathf.PI;
        }

        return result;
    }

    // Calculate the drift offset (average position) of the pattern
    private Location GetLeader() {
        // Add each assignment's contribution to the result
        Location leader;
        leader.position = _slotAssignments[0].Character.Position;
        leader.orientation = _slotAssignments[0].Character.Orientation;

        return leader;
    }

    public override bool SupportsSlots(int slotCount) {
        return slotCount <= _numberSlots;
    }
}
