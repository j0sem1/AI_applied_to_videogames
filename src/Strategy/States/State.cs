using System.Collections.Generic;
using UnityEngine;

public abstract class State {

    protected NPC _objective;
    protected bool _moving = false;

    public abstract void Action(NPC npc);
    public abstract void Execute(NPC npc);
    public abstract void CheckStatus(NPC npc);
    public abstract void Enter(NPC npc);
    public abstract void Exit(NPC npc);

    public void SetObjective(NPC newObjective) {
        _objective = newObjective;
    }

    protected bool CheckDead(NPC npc) {
        // Self-explanatory
        if (npc.Health == 0) {
            npc.ChangeState(npc.DeadState);
            return true;
        }
        return false;
    }

    protected bool CheckDefend(GameManager gameManager, NPC npc) {
        if (gameManager.EnemiesAtCheckpoint(npc) > 0 && (npc.Health > npc.LowHealth || gameManager.TotalWarMode) && !gameManager.NPCInWaypoint(npc, gameManager.WaypointManager.GetAlliedCheckpoint(npc))) {
            // If there are enemies in our capture point and I have enough health or it's total war, go defend
            npc.ChangeState(npc.DefendState);
            return true;
        }
        return false;
    }

    protected bool CheckCapture(GameManager gameManager, NPC npc) {
        if ((!npc.Patrol || npc.Patrol && gameManager.TotalWarMode) && UnitsManager.EnemiesNearby(npc) == 0 && npc.Health > npc.LowHealth) {
            // If I'm not supposed to patrol or it's total war mode and there are no nearby enemies and I have enough health
            if (gameManager.AlliesCapturing(npc) >= npc.MinAlliesForCapture) {
                // If there are enough allies capturing, go capture too
                npc.ChangeState(npc.CaptureState);
                return true;
            }
        }
        return false;
    }

    protected bool CheckEscape(NPC npc) {
        // There is no running away in total war mode
        if (npc.GameManager.TotalWarMode)
            return false;
        
        if (npc.Health <= npc.LowHealth || UnitsManager.EnemiesNearby(npc) > npc.EnemiesToRun) {
            // If I have low health or there are too many enemies, flee
            npc.ChangeState(npc.EscapeState);
            return true;
        }
        return false;
    }

    protected bool CheckMedicRangedAttack(NPC npc) {
        if (npc.Type == NPC.UnitType.Medic) {
            // If I am medic
            NPC allyChoosen = UnitsManager.ChooseAlly(npc);
            if (allyChoosen != null) {
                // If there is a wounded ally and I can "shoot" him in a straight line, "shoot" him
                npc.ChangeState(npc.RangedAttackState, allyChoosen);
                return true;
            }
        }
        return false;
    }

    // Check whether to switch to melee or ranged attack
    protected bool CheckMeleeAndRangedAttack(NPC npc) {
        List<NPC> enemies = UnitsManager.EnemiesInRange(npc);
        if (enemies != null && enemies.Count > 0) {
            // There are close enemies
            if (npc.CurrentAmmo == 0) {
                // I have no ammo
                if (UnitsManager.EnemiesNearby(npc) - UnitsManager.AlliesNearby(npc) <= npc.MaxEnemiesForMelee - npc.MinAlliesForMelee) {
                    // I have enough support to fight
                    if (!npc.GameManager.InBase(npc) && !npc.GameManager.InBase(enemies[0])) {
                        // If I am not inside the base and neither the enemy, attack said enemy
                        npc.ChangeState(npc.MeleeAttackState, enemies[0]);
                        return true;
                    }
                }
            } else {
                // I do have ammo
                foreach (NPC en in enemies) {
                    float distance = Vector3.Distance(npc.AgentNPC.Position, en.AgentNPC.Position);
                    if (distance <= npc.RangedRange && !npc.GameManager.InBase(npc) && !npc.GameManager.InBase(en)) {
                        // If the enemy is within my range, I have a straight shooting line and neither of us are in base, shoot said enemy
                        npc.ChangeState(npc.RangedAttackState, en);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    protected bool CheckReload(NPC npc) {
        if (npc.CurrentAmmo == 0 && UnitsManager.EnemiesNearby(npc) == 0) {
            // I have no ammo and there are no enemies nearby, try to reload
            npc.ChangeState(npc.ReloadState);
            return true;
        }
        return false;
    }
    
    // Get the name of the current state
    public override string ToString() {
        return GetType().Name;
    }

}