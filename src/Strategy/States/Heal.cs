using UnityEngine;

public class Heal : State {
    private bool _healed;
    private bool _pointless;
    private float _timer;
    private float _healingRate = 1;

    public override void Enter(NPC npc) {
        npc.Pathfinding.ClearPath();
        _timer = -1;
    }

    public override void Exit(NPC npc) {
        _healed = false;
        _pointless = false;
    }

    public override void Action(NPC npc) {
        // If the NPC is at base, start healing until max hp
        if (npc.GameManager.InBase(npc)) {
            if (_timer == -1)
                _timer = Time.time;

            if (Time.time - _timer >= 1) {
                _timer = -1;
                if (npc.Health < npc.MaxHealth)
                    npc.Health += 10;
                else {
                    _healed = true;
                    // You also get ammo because you are in base
                    npc.CurrentAmmo = npc.Ammo;
                }
            }

        } else {
            // Otherwise, get healed until it is acceptable by the medic
            if (npc.Health <= npc.Healthy) {
                // If the medic has died, abort
                NPC closestMedic = UnitsManager.ClosestMedic(npc);
                if (closestMedic == null || Vector3.Distance(npc.AgentNPC.Position, closestMedic.AgentNPC.Position) > closestMedic.RangedRange) {
                    _pointless = true;
                }
            } else
                _healed = true;
        }
    }

    public override void Execute(NPC npc) {
        Action(npc);
        CheckStatus(npc);
    }

    public override void CheckStatus(NPC npc) {
        if (CheckDead(npc))
            return;
        
        // Medic is dead, run to base
        if (_pointless)
            npc.ChangeState(npc.EscapeState);
        
        if (_healed) {
            npc.ChangeState(npc.IdleState);
        }

    }

}