using UnityEngine;

public class Capture : State {

    private bool _pointless;
    public override void Enter(NPC npc) {
        _moving = false;
        _pointless = false;
    }

    public override void Exit(NPC npc) {
        npc.Pathfinding.ClearPath();
    }

    public override void Action(NPC npc) {

        GameManager gameManager = npc.GameManager;
        
        // If the unit is already at the enemy capture point, stop moving
        if (gameManager.NPCInWaypoint(npc, gameManager.WaypointManager.GetEnemyCheckpoint(npc))) {
            if (_moving) {
                _moving = false;
                npc.Pathfinding.ClearPath();
            }

            // If there are no enemies defending their capture point, increment the capture bar
            if (!gameManager.EnemiesDefending(npc))
                gameManager.WaypointManager.Capturing(npc);
        } 
        // Otherwise, start moving towards the enemy capture point
        else if (!_moving) {
            // The target position is one of the random available positions within the enemy capture point
            npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, gameManager.WaypointManager.GetRandomTile(gameManager.WaypointManager.GetEnemyCheckpoint(npc)).worldPosition);
            _moving = true;
        } else {
            // If I am on my way to the enemy capture point but it happens that there are enemies attacking our point
            if (gameManager.EnemiesAtCheckpoint(npc) > 0) {
                var alliedCapturePoint = gameManager.WaypointManager.GetAlliedCheckpoint(npc).Position;
                var enemyCapturePoint = gameManager.WaypointManager.GetEnemyCheckpoint(npc).Position;
                var currentPosition = npc.CurrentTile.worldPosition;
                var distanceToEnemyCapturePoint = Vector3.Distance(alliedCapturePoint, currentPosition);
                var distanceToAlliedCapturePoint = Vector3.Distance(enemyCapturePoint, currentPosition);
                if (distanceToAlliedCapturePoint <= distanceToEnemyCapturePoint) {
                    // If I am closer to our capture point, go defend
                    // Otherwise, commit to capture
                    _pointless = true;
                }

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
        if (CheckMedicRangedAttack(npc))
            return;
        // Otherwise, check if I can continue capturing
        if (!_pointless && CheckCapture(gameManager, npc))
            return;
        npc.ChangeState(npc.IdleState);
    }

}