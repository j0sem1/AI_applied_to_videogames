using UnityEngine;

public class RangedAttack : State  {

    private float _time;
    private bool _pointless;
    public override void Enter(NPC npc) {
        _time = -1f;
        _pointless = false;
        npc.Pathfinding.ClearPath();
    }

    public override void Exit(NPC npc) {
    }

    public override void Action(NPC npc) {
        // To make a ranged attack on an enemy, I need to have a straight shooting line and the target must be within my range
        // Also, if I am low health and I am not in total war, I should not commit to shooting
        if (!UnitsManager.DirectLine(npc, _objective)
            || Vector3.Distance(npc.AgentNPC.Position, _objective.AgentNPC.Position) > npc.RangedRange
            || (!npc.GameManager.TotalWarMode && npc.Health <= npc.LowHealth))
            _pointless = true;
        if (!_pointless) {
            // Start winding up my attack
            if (_time == -1) {
                _time = Time.time;
            }
            // Wait patiently
            if (Time.time - _time >= npc.RangedChargeTime) {
                CombatManager.RangedAttack(npc, _objective);
                _time = -1;
            }
        }
    }

    public override void Execute(NPC npc) {
        Action(npc);
        CheckStatus(npc);
    }

    public override void CheckStatus(NPC npc) {
        if (CheckDead(npc))
            return;

        if (!_pointless && (CheckMedicRangedAttack(npc) || CheckMeleeAndRangedAttack(npc)))
            return;
        
        npc.ChangeState(npc.IdleState);
    }
}