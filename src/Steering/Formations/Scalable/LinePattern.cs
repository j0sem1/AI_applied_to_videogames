using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePattern : FormationManager {
    // The radius of one character, this is needed to determine how
    // close we can pack a given number of characters around a circle
    [SerializeField]
    private float _characterRadius;

    [SerializeField]
    private float _numberSlots;

    // Send new target locations to each character
    public override void UpdateSlots() {
        // Find the anchor point
        Location anchor = GetAnchor();
        // For a reason past the developer's comprehension, inverting twice the orientation makes it work
        anchor.orientation *= -1; 

        // Go throught each character in turn
        int slotAssignmentsSize = _slotAssignments.Count;
        for (int i = 0; i < slotAssignmentsSize; i++)
        {
            int slotNumber = _slotAssignments[i].SlotNumber;
            Location slot = GetSlotLocation(slotNumber);

            // Transform by the anchor point position and orientation
            Location location;
            location.position = Vector3.zero;
            location.orientation = 0f;
            // Rotation Matrix
            var result = new Vector3(Mathf.Cos(anchor.orientation) * slot.position.x -  Mathf.Sin(anchor.orientation) * slot.position.z,
                0,
                 Mathf.Sin(anchor.orientation) * slot.position.x + Mathf.Cos(anchor.orientation) * slot.position.z);
            location.position = anchor.position + result;
            location.orientation = -(anchor.orientation + slot.orientation);

            // Set the position with seek
            _slotAssignments[i].Character.GetComponent<Seek>().Target = location.position;
            // Set the orientation
            _slotAssignments[i].Character.GetComponent<Align>().TargetOrientation = location.orientation;
        }
    }

    // Calculate the position of a slot
    public override Location GetSlotLocation(int slotNumber) {
        Location result;

        float positionInLine = slotNumber * _characterRadius;
        result.position = new Vector3(positionInLine, 0, 0);
        result.orientation = 0;

        return result;
    }

    // Calculate the anchor as the average position and orientation of the formation
    private Location GetAnchor() {

        // Add each assignment's contribution to the result
        Location center;
        center.position = Vector3.zero;
        center.orientation = 0f;

        Vector3 averagePosition = Vector3.zero;
        float averageOrientation = 0f;
        
        foreach (var assignment in _slotAssignments) {
            Location location = GetSlotLocation(assignment.SlotNumber);
            center.position += location.position;
            center.orientation += location.orientation;

            averagePosition += assignment.Character.Position;
            averageOrientation += assignment.Character.Orientation;
        }

        // Divide through to get the drift offset
        int numberOfAssignments = _slotAssignments.Count;
        center.position /= numberOfAssignments;
        center.orientation /= numberOfAssignments;

        averagePosition /= numberOfAssignments;
        averageOrientation /= numberOfAssignments;
        center.position += averagePosition;
        center.orientation += averageOrientation;

        return center;
    }

    
    public override bool SupportsSlots(int slotCount) {
        // We support any number of slots
        return true;
    }
}
