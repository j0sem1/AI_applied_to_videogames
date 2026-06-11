using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;
using System.Linq;

public static class UnitsManager {

    private static int _npcLayerMask = 1 << 9;

    // Defines the range for each of the OverlapShere calls used in this script
    // Essentially, it is the distance at which a unit can "see"
    private static int _sphereRange = 25;

    // Returns the number of enemies near that the unit can see
    public static int EnemiesNearby(NPC npc) {
        int result = 0;
        Collider[] hitColliders = Physics.OverlapSphere(npc.AgentNPC.Position, 5, _npcLayerMask);
        int i = 0;
        while (i < hitColliders.Length) {
            NPC actualNPC = hitColliders[i].GetComponent<NPC>();
            if (actualNPC != null && actualNPC.Team != npc.Team && !actualNPC.IsDead && DirectLine(npc, actualNPC))
                result++;
            i++;
        }
        return result;
    }

    // Returns the number of allies near that the unit can see
    public static int AlliesNearby(NPC npc) {
        int result = 0;
        Collider[] hitColliders = Physics.OverlapSphere(npc.AgentNPC.Position, 5, _npcLayerMask);
        int i = 0;
        while (i < hitColliders.Length) {
            NPC actualNPC = hitColliders[i].GetComponent<NPC>();
            if (actualNPC != null && actualNPC.Team == npc.Team && !actualNPC.IsDead && DirectLine(npc, actualNPC))
                result++;
            i++;
        }
        return result;
    }
    
    // Returns all enemies within the visible range ordered by distance
    public static List<NPC> EnemiesInRange(NPC npc) {
        List<NPC> enemies = new List<NPC>();
        Collider[] hitColliders = Physics.OverlapSphere(npc.AgentNPC.Position, npc.RangedRange, _npcLayerMask);
        int i = 0;
        while (i < hitColliders.Length) {
            NPC actualNPC = hitColliders[i].GetComponent<NPC>();
            if (actualNPC != null && actualNPC.Team != npc.Team && DirectLine(npc, actualNPC) && !actualNPC.IsDead) {
                enemies.Add(actualNPC);
            }
            i++;
        }
        if (enemies.Count > 0) {
            enemies = enemies.OrderBy(e => Vector3.Distance(e.AgentNPC.Position, npc.AgentNPC.Position)).ToList();
        }
        return enemies;
    }

    // Returns the closest medic alive to the unit
    public static NPC ClosestMedic(NPC npc) {
        Collider[] hitColliders = Physics.OverlapSphere(npc.AgentNPC.Position, _sphereRange, _npcLayerMask);
        int i = 0;
        float minimalDistance = float.MaxValue;
        NPC selected = null;
        while (i < hitColliders.Length) {
            NPC actualNPC = hitColliders[i].GetComponent<NPC>();
            if (actualNPC.Team == npc.Team && actualNPC.Type == NPC.UnitType.Medic && !actualNPC.IsDead) {
                float distance = Vector3.Distance(actualNPC.AgentNPC.Position, npc.AgentNPC.Position);
                if (distance < minimalDistance) {
                    minimalDistance = distance;
                    selected = actualNPC;
                }
            }
            i++;
        }
        return selected;
    }
    
    // Returns the closes ally, if any
    public static NPC ClosestAlly(NPC npc) {
        Collider[] hitColliders = Physics.OverlapSphere(npc.AgentNPC.Position, _sphereRange, _npcLayerMask);
        int i = 0;
        float minimalDistance = float.MaxValue;
        NPC allySelected = null;
        while (i < hitColliders.Length) {
            NPC actualNPC = hitColliders[i].GetComponent<NPC>();
            if (actualNPC != null && actualNPC.Team == npc.Team && !actualNPC.IsDead) {
                float distance = Vector3.Distance(actualNPC.AgentNPC.Position, npc.AgentNPC.Position);
                if (distance < minimalDistance) {
                    minimalDistance = distance;
                    allySelected = actualNPC;
                }  
            }
            i++;
        }

        if (allySelected != null)
            return allySelected;
        return null;
    }
    
    // Find the ally with the lowest health between all close units that are "LowHealth"
    public static NPC ChooseAlly(NPC npc) {
        List<NPC> lowHealth = new List<NPC>();
        Collider[] hitColliders = Physics.OverlapSphere(npc.AgentNPC.Position, npc.RangedRange, _npcLayerMask);
        int i = 0;
        while (i < hitColliders.Length) {
            NPC actualNPC = hitColliders[i].GetComponent<NPC>();
            if (actualNPC != null && actualNPC.Team == npc.Team && DirectLine(npc, actualNPC) && !actualNPC.IsDead) {
                if (actualNPC.Health <= actualNPC.LowHealth)
                    lowHealth.Add(actualNPC);
            }
            i++;
        }
        if (lowHealth.Count > 0) {
            lowHealth = lowHealth.OrderBy(e => e.Health).ToList();
            return lowHealth[0];
        }
        return null;
    }

    // Are there any obstacles between the attacker and the target?
    public static bool DirectLine(NPC attacker, NPC attacked) {
        RaycastHit hit;
        var direction = attacked.AgentNPC.Position - attacker.AgentNPC.Position;
        if (Physics.Raycast(attacker.AgentNPC.Position, direction, out hit))
            return hit.collider.GetComponent<NPC>() == attacked;
        return false;
    }

}
