using UnityEngine;

public class User : State  {

    public override void Enter(NPC npc) {

    }

    public override void Exit(NPC npc) {
        npc.Pathfinding.ClearPath();
    }

    public override void Action(NPC npc) {

    }

    public override void Execute(NPC npc) {
        Action(npc);
        CheckStatus(npc);
    }

    public override void CheckStatus(NPC npc) {
        // Leave manual control once the target destination has been reached or if the unit is dead
        if (CheckDead(npc))
            return;
        if (Vector3.Distance(npc.AgentNPC.Position, npc.EndPath) < 0.5f)
            npc.ChangeState(npc.IdleState);
    }

}