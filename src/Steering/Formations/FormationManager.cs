using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FormationManager : MonoBehaviour {
    
    [System.Serializable]
    public struct Location {
        public float orientation;
        public Vector3 position;
    }

    // The assigment characters to slots
    public class SlotAssignment {
        private AgentNPC _character;
        private int _slotNumber;

        public AgentNPC Character {
            get => _character;
            set => _character = value;
        }

        public int SlotNumber {
            get => _slotNumber;
            set => _slotNumber = value;
        }
    }

    [SerializeField]
    protected List<AgentNPC> _agents = new List<AgentNPC>();

    protected List<SlotAssignment> _slotAssignments;

    void Start() {
        _slotAssignments = new List<SlotAssignment>();

        // Add the agents to the formation
        foreach (AgentNPC ag in _agents) {
            AddCharacter(ag);
        }

        UpdateSlots();
    }
    
    // Update the assigned number to each slot
    public void UpdateSlotAssignments() {
        for (int i = 0; i <  _slotAssignments.Count; i++) {
            _slotAssignments[i].SlotNumber = i;
        }
    }

    // Add a new character. Return false if no slots are available
    public bool AddCharacter(AgentNPC character) {
        // Check if the pattern supports more slots
        int occupiedSlots = _slotAssignments.Count;
        if (SupportsSlots(occupiedSlots + 1)) {
            // Add a new slot assignment
            SlotAssignment slotAssignment = new SlotAssignment();
            slotAssignment.Character = character;
            _slotAssignments.Add(slotAssignment);
            UpdateSlotAssignments();
            return true;
        } else {
            // Otherwise we've failed to add the character
            return false;
        }
    }

    // Remove a character from its slot
    public void RemoveCharacter(AgentNPC character) {
        foreach (SlotAssignment slotAssignment in _slotAssignments) {
            if (character.Equals(slotAssignment.Character))
                _slotAssignments.Remove(slotAssignment);
        }
        UpdateSlotAssignments();
    }

    // Send new target locations to each character
    public abstract void UpdateSlots();

    // Calculates the position of a slot
    public abstract Location GetSlotLocation(int slotNumber);

    // Return true if formation supports a certain number of slots
    public abstract bool SupportsSlots(int slotCount);
}
