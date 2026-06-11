using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    [SerializeField]
    private GridMap _gridMap;
    public GridMap GridMap => _gridMap;

    [SerializeField]
    private List<NPC> _npcs;

    [SerializeField]
    private bool _totalWarMode;
    public bool TotalWarMode => _totalWarMode;

    [SerializeField]
    private WaypointManager _waypointManager;
    public WaypointManager WaypointManager => _waypointManager;

    [SerializeField]
    private StrategyInputManager _strategyInputManager;

    private float minDistance = 1.5f;

    [SerializeField]
    // Set the minimum speed of movement required for capture the enemy checkpoint
    private float _speedForCapturing;
    public float SpeedForCapturing => _speedForCapturing;

    [SerializeField] private GameObject _redVictory;
    [SerializeField] private GameObject _bluVictory;
    [SerializeField] private GameObject _restart;

    // Start is called before the first frame update
    void Start()
    {
        foreach (NPC npc in _npcs) {
            npc.GameManager = this;
            npc.GridMap = _gridMap;
        }
        _waypointManager.GameManager = this;
        _waypointManager.GridMap = _gridMap;
        _strategyInputManager.GameManager = this;
    }
    
    void Update() {
        bool bluCapturing = false;
        bool redCapturing = false;
        foreach (NPC npc in _npcs) {
            if (!npc.IsDead && NPCInWaypoint(npc, WaypointManager.GetEnemyCheckpoint(npc))) {
                if (npc.Team == NPC.UnitTeam.Red)
                    redCapturing = true;
                else
                    bluCapturing = true;
            }
        }
        if (!redCapturing) {
            WaypointManager.RedTeamNotCapturing();
        }
        if (!bluCapturing) {
            WaypointManager.BluTeamNotCapturing();
        }
    }
    
    public int EnemiesAtCheckpoint(NPC npc) {
        int enemiesAtCheckpoint = 0;
        foreach (NPC npc2 in _npcs) {
            if (npc2.Team != npc.Team && !npc2.IsDead) {
                
                foreach (Transform position in WaypointManager.GetAlliedCheckpoint(npc).Positions) {
                    if (Vector3.Distance(npc2.AgentNPC.Position, position.position) <= minDistance)
                        enemiesAtCheckpoint++;
                }
            }
        }
        return enemiesAtCheckpoint;
    }

    public int AlliesCapturing(NPC npc) {
        int alliesCapturing = 0;
        foreach (NPC npc2 in _npcs) {
            if (npc2.Team == npc.Team && !npc2.IsDead) {
                foreach (Transform position in WaypointManager.GetEnemyCheckpoint(npc).Positions) {
                    if (Vector3.Distance(npc2.AgentNPC.Position, position.position) <= minDistance)
                        alliesCapturing++;
                }
            }
        }
        return alliesCapturing;
    }

    public bool EnemiesDefending(NPC npc) {
        Waypoint enemyCheckpoint = WaypointManager.GetEnemyCheckpoint(npc);
        foreach (NPC npc2 in _npcs) {
            if(npc2.Team != npc.Team && !npc2.IsDead) {
                if (NPCInWaypoint(npc2, enemyCheckpoint))
                    return true;
            }
        }
        return false;
    }

    // Return true if the NPC is on base
    public bool InBase(NPC npc) {
        foreach (Transform position in WaypointManager.GetAlliedBase(npc).Positions) {
            if (Vector3.Distance(npc.AgentNPC.Position, position.position) <= minDistance)
                return true;
        }
        return false;
    }

    public bool NPCInWaypoint(NPC npc, Waypoint waypoint) {
        foreach (Transform position in waypoint.Positions) {
            if (Vector3.Distance(npc.AgentNPC.Position, position.position) <= minDistance)
                return true;
        }
        return false;
    }

    public void ToggleOffensiveMode(NPC.UnitTeam team) {
        foreach (NPC npc in _npcs)
            if (npc.Team == team)
                npc.ToggleOffensiveMode();
    }

    public void ToggleDefensiveMode(NPC.UnitTeam team) {
        foreach (NPC npc in _npcs)
            if (npc.Team == team)
                npc.ToggleDefensiveMode();
    }

    public void EnableTotalWar() {
        _totalWarMode = true;
        foreach (NPC npc in _npcs)
            npc.ToggleTotalWar();
    }

    public void BlueWins() {
        _bluVictory.SetActive(true);
        _restart.SetActive(true);
        Time.timeScale = 0;
    }

    public void RedWins() {
        _redVictory.SetActive(true);
        _restart.SetActive(true);
        Time.timeScale = 0;
    }

    public void Restart() {
        _waypointManager.Restart();
        _redVictory.SetActive(false);
        _bluVictory.SetActive(false);
        _restart.SetActive(false);
        foreach (var npc in _npcs) {
            npc.Restart();
        }
        Time.timeScale = 1;
    }

    public void MuteUnmuteMusic() {
        AudioSource audio = GetComponent<AudioSource>();

        if (audio.isPlaying)
            audio.Stop();
        else
            audio.Play();
    }
}
