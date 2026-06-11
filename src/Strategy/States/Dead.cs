using UnityEngine;

public class Dead : State  {

    private float _deadTime = 10f;
    private float _time;

    public override void Enter(NPC npc) {
        npc.SimplePropagator.Value = 0;
        npc.Pathfinding.ClearPath();
        _moving = false;
        _time = Time.time;
    }

    public override void Exit(NPC npc) {
        if (npc.Team == NPC.UnitTeam.Red) 
            npc.SimplePropagator.Value = 1;
        else
            npc.SimplePropagator.Value = -1;
    }

    public override void Action(NPC npc) {
        // When it is finally time to come back from the dead, respawn at base
        if (Time.time - _time >= _deadTime) {
            npc.AgentNPC.Position = npc.GameManager.WaypointManager.GetRandomTile(npc.GameManager.WaypointManager.GetAlliedBase(npc)).worldPosition;
            npc.Health = npc.MaxHealth;
            npc.CurrentAmmo = npc.Ammo;
        }
    }

    public override void Execute(NPC npc) {
        Action(npc);
        CheckStatus(npc);
    }

    public override void CheckStatus(NPC npc) {
        // I have returned from the dead, get back to business
        if (npc.Health > 0)
            npc.ChangeState(npc.IdleState);
    }
}