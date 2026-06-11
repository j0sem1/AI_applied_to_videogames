using UnityEngine;

public class Patrol : State  {

    private bool _patrolling;
    private float minDistance = 2f;
    public override void Enter(NPC npc) {
        _moving = false;
        _patrolling = false;
        npc.Pathfinding.Patrol = true;
    }

    public override void Exit(NPC npc) {
        npc.Pathfinding.Patrol = false;
        npc.Pathfinding.ClearPath();
    }

    public override void Action(NPC npc) {
        float distanceToBeginning = Vector3.Distance(npc.AgentNPC.Position, npc.BeginningPoint.position);
        float distanceToEnd = Vector3.Distance(npc.AgentNPC.Position, npc.EndPoint.position);
        if (!_moving) {
            if (distanceToBeginning < distanceToEnd)
                npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, npc.BeginningPoint.position);
            else 
                npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, npc.EndPoint.position);
            _moving = true;
        }
        else if (distanceToBeginning <= minDistance || distanceToEnd <= minDistance) {
            if (!_patrolling) {
                npc.Pathfinding.Patrol = true;
                if (distanceToBeginning <= minDistance)
                    npc.Pathfinding.FindPathToPosition(npc.BeginningPoint.position, npc.EndPoint.position);
                else if (distanceToEnd <= minDistance)
                    npc.Pathfinding.FindPathToPosition(npc.EndPoint.position, npc.BeginningPoint.position);
                _patrolling = true;
            }
        }
    }

    public override void Execute(NPC npc) {
        Action(npc);
        CheckStatus(npc);
    }

    public override void CheckStatus(NPC npc) {

        GameManager gameManager = npc.GameManager;

        // If the unit is dead, change to that state
        if (CheckDead(npc))
            return;
        // If there are too many enemies, flee
        if (CheckEscape(npc))
            return;
        // Otherwise, if I am a medic, check if there are nearby allies to "shoot" (heal) and if so, shoot them
        if (CheckMedicRangedAttack(npc))
            return;
        // Otherwise, check first if I have to defend my capture point
        if (CheckDefend(gameManager, npc))
            return;
        // Otherwise, check if I can capture the enemy point
        if (CheckCapture(gameManager, npc))
            return;
        // Otherwise, check if I can attack any enemies
        if (CheckMeleeAndRangedAttack(npc))
            return;
        // Otherwise, check if I have to reload
        if (CheckReload(npc))
            return;
    }
}