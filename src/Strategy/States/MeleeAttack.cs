using UnityEngine;

public class MeleeAttack : State  {

    private float _time;
    private bool _iTried;
    private bool _pointless;
    public override void Enter(NPC npc) {
        _moving = false;
        _iTried = false;
        _pointless = false;
        _time = -1;
    }

    public override void Exit(NPC npc) {
        npc.Pathfinding.ClearPath();
    }

    public override void Action(NPC npc) {
        
        // To melee attack an enemy, he must be within my range
        float distance = Vector3.Distance(npc.AgentNPC.Position, _objective.AgentNPC.Position);
        if (distance <= npc.MeleeRange) {

            // He is within my range, stop moving
            if (_moving) {
                _moving = false;
                npc.Pathfinding.ClearPath();
            }
            // I can start winding up my attack
            if (_time == -1) {
                if (!npc.GameManager.TotalWarMode && npc.Health <= npc.LowHealth) {
                    // But I am on low HP and I am not in total war, so I should leave
                    _pointless = true;
                    return;
                }
                _time = Time.time;
            }
            
            // Wait patiently
            if (Time.time - _time >= npc.MeleeAttackSpeed) {
                CombatManager.MeleeAttack(npc, _objective);
                _time = -1;
                _iTried = false;
            }
        } else {
            // He is not within my range
            if (!npc.GameManager.TotalWarMode && npc.Health <= npc.LowHealth) {
                // I happen to be low health and I am not in total war, so it's not worth commiting
                _pointless = true;
                return;
            }
            
            if (!_moving && !_iTried && _time == -1) {
                // I'm not moving and I haven't tried chasing him
                npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, _objective.CurrentTile.worldPosition);
                _moving = true;
                _iTried = true;
            }
            else if (npc.Pathfinding.EndOfThePath() && _iTried) {
                _pointless = true;
            }
        }
    }

    public override void Execute(NPC npc) {
        Action(npc);
        CheckStatus(npc);
    }

    public override void CheckStatus(NPC npc) {
        
        // If the unit is dead, change to that state
        if (CheckDead(npc))
            return;
        if (!_pointless && CheckMeleeAndRangedAttack(npc))
            return;
        npc.ChangeState(npc.IdleState);
    }


}