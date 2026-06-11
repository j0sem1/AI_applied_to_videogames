using UnityEngine;

public class Reload : State  {

    private float _time;
    private Tile _nearestCover;
    private bool _pointless;
    public override void Enter(NPC npc) {
        _moving = false;
        _pointless = false;
        _nearestCover = npc.GameManager.WaypointManager.GetNearestCover(npc);
        _time = -1;
    }

    public override void Exit(NPC npc) {
        npc.Pathfinding.ClearPath();
    }

    public override void Action(NPC npc) {
        // If we're close enough to our cover point
        if (npc.CurrentTile == _nearestCover) {
            if (_time == -1 && UnitsManager.EnemiesNearby(npc) > 0) {
                // If I have reached to cover point and there are enemies there, return to base to reload
                _nearestCover =
                    npc.GameManager.WaypointManager.GetRandomTile(npc.GameManager.WaypointManager.GetAlliedBase(npc));
                npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, _nearestCover.worldPosition);
                return;
            }
            
            // Start reloading
            if (_time == -1) {
                _time = Time.time;
            }
            
            if (Time.time - _time >= npc.ReloadTime) {
                npc.CurrentAmmo = npc.Ammo;
                _time = -1;
            }
        } else {
            if (!_moving) {
                npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, _nearestCover.worldPosition);
                _moving = true;
            }
            if (_moving && UnitsManager.EnemiesNearby(npc) > 0) {
                _pointless = true;
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
        if (npc.CurrentAmmo == npc.Ammo || _pointless)
            npc.ChangeState(npc.IdleState);

    }
}