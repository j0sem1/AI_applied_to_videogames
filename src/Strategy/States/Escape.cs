using UnityEngine;

public class Escape : State {

    private bool _goHeal;
    private bool _pointless;
    private bool _lowHealth;
    private NPC _closestMedic;
    private Tile _alliedBase;
    private Tile _startingAllyTile;
    private Tile _startingMedicTile;
    private float _distanceToMedic;
    private float _distanceToAlly;
    private float _distanceToBase;
    
    public override void Enter(NPC npc) {
        _lowHealth = npc.Health <= npc.LowHealth;
        NPC closestAlly = null;
        if (_lowHealth) {
            _closestMedic = UnitsManager.ClosestMedic(npc);
        }
        else {
            closestAlly = UnitsManager.ClosestAlly(npc);
        }
        _alliedBase = npc.GameManager.WaypointManager.GetRandomTile(npc.GameManager.WaypointManager.GetAlliedBase(npc));
        
        _distanceToBase = Vector3.Distance(npc.CurrentTile.worldPosition, _alliedBase.worldPosition);
        if (_closestMedic) {
            _distanceToMedic = Vector3.Distance(_closestMedic.AgentNPC.Position, npc.AgentNPC.Position);
            _startingMedicTile = _closestMedic.CurrentTile;
        }
        else
            _distanceToMedic = float.MaxValue;

        if (closestAlly != null) {
            _startingAllyTile = closestAlly.CurrentTile;
            _distanceToAlly = Vector3.Distance(npc.CurrentTile.worldPosition, _startingAllyTile.worldPosition);
        }
        else
            _distanceToAlly = float.MaxValue;
        _moving = false;
        _goHeal = false;
        _pointless = false;
    }

    public override void Exit(NPC npc) {
        npc.Pathfinding.ClearPath();
    }

    public override void Action(NPC npc) {
        // There are two reasons to escape: too many enemies or low health
        if (_lowHealth) {
            // Low health, decide whether to go to base or to a medic
            // There are two possible routes to escape: my base, the closest ally or a medic
            if (!_moving) {
                _moving = true;
                if (_distanceToMedic < _distanceToBase && npc.Type != NPC.UnitType.Medic) {
                    // If I am closer to the medic, go to the medic
                    npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, _closestMedic.CurrentTile.worldPosition);
                    return;
                }
                // Otherwise, go to base
                npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, _alliedBase.worldPosition);
            } else {
                if (_distanceToMedic < _distanceToBase && npc.Type != NPC.UnitType.Medic) {
                    // I was headed towards my medic
                    if (npc.CurrentTile == _startingMedicTile) {
                        // I have reached where my medic is supposed to be
                        NPC currentClosestMedic = UnitsManager.ClosestMedic(npc);
                        if (currentClosestMedic == null || Vector3.Distance(npc.AgentNPC.Position, currentClosestMedic.AgentNPC.Position) > currentClosestMedic.RangedRange || currentClosestMedic.CurrentAmmo == 0) {
                            // My medic isn't there or he has no ammo, then think again
                            _pointless = true;
                        }
                        else {
                            // Let yourself get healed
                            _goHeal = true;
                        }
                    }
                }
                else {
                    // I was headed towards base
                    if (npc.CurrentTile == _alliedBase) {
                        // I have reached my position
                        _goHeal = true;
                    }
                }
            }
        }
        else {
            // Too many enemies
            // There are two possible routes to escape: my base or the closest ally
            if (!_moving) {
                _moving = true;
                if (_distanceToAlly < _distanceToBase) {
                    // If I am closer to the last known position of the ally, go there
                    npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, _startingAllyTile.worldPosition);
                    return;
                }

                // Otherwise, go to base
                npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, _alliedBase.worldPosition);
            } else {
                if (_distanceToAlly < _distanceToBase) {
                    // I was headed to an ally
                    if (npc.CurrentTile == _startingAllyTile) {
                        // I have reached my destination
                        if (UnitsManager.EnemiesNearby(npc) > 0) {
                            // There are enemies at the position of my ally, escape to base 
                            _startingAllyTile = _alliedBase;
                            npc.Pathfinding.ClearPath();
                            npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, _alliedBase.worldPosition);
                        }
                        else {
                            if (_startingAllyTile == _alliedBase) {
                                // The first attempt to escape failed, we are now at base, so might as well heal
                                _goHeal = true;
                            }
                            else {
                                _pointless = true;
                            }
                        }
                    }
                }
                else {
                    // I was headed to base
                    if (npc.CurrentTile == _alliedBase) {
                        // I have reached my destination, might as well heal
                        _goHeal = true;
                    }
                }
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
        
        if (_goHeal)
            npc.ChangeState(npc.HealState);
        
        if (_pointless)
            npc.ChangeState(npc.IdleState);
        
    }

}