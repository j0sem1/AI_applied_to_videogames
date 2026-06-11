public class Idle : State  {

    public override void Enter(NPC npc) {

    }

    public override void Exit(NPC npc) {

    }

    public override void Action(NPC npc) {

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
        
        // If I cannot do any of these tasks and I have to patrol, then patrol
        if (npc.Patrol)
            npc.ChangeState(npc.PatrolState);
        else {
            // Otherwise, roam around our "country"
            npc.ChangeState(npc.RoamState);
        }
    }

}