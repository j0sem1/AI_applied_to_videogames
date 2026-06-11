public class Roam : State {

    public override void Enter(NPC npc) {
        _moving = false;
    }

    public override void Exit(NPC npc) {
        npc.Pathfinding.ClearPath();
    }

    public override void Action(NPC npc) {

        GameManager gameManager = npc.GameManager;
        if (!_moving) {
            if (npc.Team == NPC.UnitTeam.Red)
                npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, gameManager.WaypointManager.GetRandomTile(gameManager.WaypointManager.RedRoamWaypoint).worldPosition);
            else
                npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, gameManager.WaypointManager.GetRandomTile(gameManager.WaypointManager.BluRoamWaypoint).worldPosition);
            _moving = true;
        }
        if (npc.Pathfinding.EndOfThePath())
            _moving = false;
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