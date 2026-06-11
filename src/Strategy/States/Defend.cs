public class Defend : State  {

    public override void Enter(NPC npc) {
        _moving = false;
    }

    public override void Exit(NPC npc) {
        npc.Pathfinding.ClearPath();
    }

    public override void Action(NPC npc) {

        if (!_moving) {
            // Go to our capture point
            npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, npc.GameManager.WaypointManager.GetRandomTile(npc.GameManager.WaypointManager.GetAlliedCheckpoint(npc)).worldPosition);
            _moving = true;
        }

    }

    public override void Execute(NPC npc) {
        Action(npc);
        CheckStatus(npc);
    }

    public override void CheckStatus(NPC npc) {
        GameManager gameManager = npc.GameManager;

        if (CheckDead(npc))
            return;
        if (CheckMedicRangedAttack(npc))
            return;
        if (CheckDefend(gameManager, npc))
            return;
            
        npc.ChangeState(npc.IdleState);
    }

}