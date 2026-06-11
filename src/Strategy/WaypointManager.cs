using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{

    private GridMap _gridMap;
    public GridMap GridMap {
        get => _gridMap;
        set => _gridMap = value;
    }

    private GameManager _gameManager;
    public GameManager GameManager {
        get => _gameManager;
        set => _gameManager = value;
    }

    [SerializeField]
    private Waypoint _blueBaseWaypoint;
    [SerializeField]
    private Waypoint _redBaseWaypoint;
    [SerializeField]
    private Waypoint _redCheckpointWaypoint;
    [SerializeField]
    private Waypoint _blueCheckpointWaypoint;
    [SerializeField]
    private Waypoint[] _coverWaypoints;
    [SerializeField] 
    private Waypoint _redRoamWaypoint;
    public Waypoint RedRoamWaypoint => _redRoamWaypoint;
    [SerializeField] 
    private Waypoint _bluRoamWaypoint;
    public Waypoint BluRoamWaypoint => _bluRoamWaypoint;
    // Start is called before the first frame update

    public Tile GetRandomTile(Waypoint waypoint) {
        int random = Random.Range(0, waypoint.Positions.Length);
        return _gridMap.GetTile(waypoint.Positions[random].position);
    }

    public Waypoint GetAlliedBase(NPC npc) {
        // Check if our NPC is from Blu or Red team.
        if (npc.Team == NPC.UnitTeam.Blu)
            return _blueBaseWaypoint;
        return _redBaseWaypoint;
    }

    public Waypoint GetAlliedCheckpoint(NPC npc) {
        // Check if our NPC is from Blu or Red team.
        if (npc.Team == NPC.UnitTeam.Blu)
            return _blueCheckpointWaypoint;
        return _redCheckpointWaypoint;
    }

    public Waypoint GetEnemyCheckpoint(NPC npc) {
        // Check if our NPC is from Blu or Red team.
        if (npc.Team == NPC.UnitTeam.Blu)
            return _redCheckpointWaypoint;
        return _blueCheckpointWaypoint;
    }

    public Tile GetNearestCover(NPC npc) {
        float minimalDistance = float.MaxValue;
        Vector3 nearestCover = Vector3.zero;
        foreach (Waypoint waypoint in _coverWaypoints) {
            float distance = Vector3.Distance(npc.GetComponent<AgentNPC>().Position, waypoint.Position);
            if (distance < minimalDistance) {
                minimalDistance = distance;
                nearestCover = waypoint.Position;
            }
        }
        return _gridMap.GetTile(nearestCover);
    }

    public void Capturing(NPC npc) {
        if (npc.Team == NPC.UnitTeam.Red) {
            _blueCheckpointWaypoint.CapturePercentage += npc.CaptureRate * Time.deltaTime;
            if (_blueCheckpointWaypoint.CapturePercentage >= 100)
                _gameManager.BlueWins();
        } else {
            _redCheckpointWaypoint.CapturePercentage += npc.CaptureRate * Time.deltaTime;
            if (_redCheckpointWaypoint.CapturePercentage >= 100)
                _gameManager.RedWins();
        }
    }

    public void RedTeamNotCapturing() {
        if (_blueCheckpointWaypoint.CapturePercentage > 0)
            _blueCheckpointWaypoint.CapturePercentage -= 0.5f * Time.deltaTime;
    }

    public void BluTeamNotCapturing() {
        if (_redCheckpointWaypoint.CapturePercentage > 0)
            _redCheckpointWaypoint.CapturePercentage -= 0.5f * Time.deltaTime;
    }

    public void Restart() {
        _blueCheckpointWaypoint.CapturePercentage = 0;
        _redCheckpointWaypoint.CapturePercentage = 0;
    }
}
