using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Steering.Pathfinding;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour  {
    
    public enum UnitType {
        Scout,
        Heavy,
        Sniper,
        Medic
    }

    public enum UnitTeam {
        Red,
        Blu
    }

    [Header("References")] 
    private GridMap _gridMap;
    private GameManager _gameManager;
    [SerializeField] private AgentNPC _agentNPC;
    private SimplePropagator _simplePropagator;
    [SerializeField] private PathFinding _pathfinding;
    
    public GameManager GameManager {
        get => _gameManager;
        set => _gameManager = value;
    }

    public GridMap GridMap {
        get => _gridMap;
        set => _gridMap = value;
    }
    public AgentNPC AgentNPC => _agentNPC;
    public SimplePropagator SimplePropagator => _simplePropagator;

    public PathFinding Pathfinding {
        get => _pathfinding;
        set => _pathfinding = value;
    }

    // NPC static information
    [Header("Unit information")]
    
    [SerializeField] protected UnitType _unitType;
    [SerializeField] protected UnitTeam _team;
    [SerializeField] protected string _name;
    public UnitType Type {
        get => _unitType;
        set => _unitType = value;
    }
    public UnitTeam Team {
        get => _team;
        set => _team = value;
    }
    public string Name {
        get => _name;
        set => _name = value;
    }

    [Header("Unit values")]
    [SerializeField] protected int _maxHealth;
    // Melee
    [SerializeField] protected int _meleeDamage;
    [SerializeField] protected int _meleeCritDamage;
    [SerializeField] protected float _meleeAttackSpeed;
    [SerializeField] protected float _meleeRange;
    // Ranged
    [SerializeField] protected int _rangedDamage;
    [SerializeField] protected int _rangedCritDamage;
    [SerializeField] protected float _rangedChargeTime;
    [SerializeField] protected float _rangedRange;
    // Ammo
    [SerializeField] protected int _ammo;
    [SerializeField] protected float _reloadTime;
    [SerializeField] protected int _ammoPerShot;
    // Speed
    [SerializeField] protected float _speed;
    // Patrol
    [SerializeField] protected bool _patrol;
    [SerializeField] protected Transform _beginningPoint;
    [SerializeField] protected Transform _endPoint;
    //Capture
    [SerializeField] protected int _captureRate;
    //Initial Weights
    [SerializeField] protected int _initialEnemiesToRun;
    [SerializeField] protected int _initialLowHealth;
    [SerializeField] protected float _initialHealthy;
    [SerializeField] protected int _initialMaxEnemiesForMelee;
    [SerializeField] protected int _initialMinAlliesForMelee;
    [SerializeField] protected int _initialAlliesForCapture;
     
    public int Health {
        get => _currentHealth;
        set {
            GUIManager.ShowHealthChange(this, value - _currentHealth);
            _currentHealth = value;
            if (_currentHealth < 0) {
                // Dead
                _currentHealth = 0;

            }
            else if (_currentHealth > _maxHealth) {
                // Full HP
                _currentHealth = _maxHealth;
            }

            if (_currentHealth != _currentHealthAnimation && _healthAnimationCoroutine == null)
                _healthAnimationCoroutine = StartCoroutine(AnimateHealthChange());

        }
    }

    public int MaxHealth => _maxHealth;
    public int MeleeDamage => _meleeDamage;
    public int MeleeCritDamage => _meleeCritDamage;
    public float MeleeAttackSpeed => _meleeAttackSpeed;
    public float MeleeRange => _meleeRange;
    public int RangedDamage => _rangedDamage;
    public int RangedCritDamage => _rangedCritDamage;
    public float RangedChargeTime => _rangedChargeTime;
    public float RangedRange => _rangedRange;
    public float ReloadTime => _reloadTime;
    public int Ammo => _ammo;
    public int AmmoPerShot => _ammoPerShot;
    public int CurrentAmmo {
        get => _currentAmmo;
        set {
            _currentAmmo = value;
            if (_currentAmmo < 0) {
                _currentAmmo = 0;

            }
            else if (_currentAmmo > _ammo) {
                // Full HP 
                _currentAmmo = _ammo;
            }

            _ammoBar.UpdateBar(_currentAmmo, _ammo);
        }
    }
    public float Speed => _speed;
    public bool Patrol {
        get => _patrol;
        set => _patrol = value;
    }
    public Transform BeginningPoint => _beginningPoint;
    public Transform EndPoint => _endPoint;
    public int CaptureRate => _captureRate;

    [Header("Tactical information")]
    // Tactical information
    [SerializeField] protected int _enemiesToRun;
    [SerializeField] private int _lowHealth;
    [SerializeField] private float _healthy;
    [SerializeField] private int _maxEnemiesForMelee;
    [SerializeField] private int _minAlliesForMelee;
    [SerializeField] private int _minAlliesForCapture;
    private float _groupSpeed;

    public int EnemiesToRun => _enemiesToRun;
    public int LowHealth => _lowHealth;
    public float Healthy => _healthy;
    public int MaxEnemiesForMelee => _maxEnemiesForMelee;
    public int MinAlliesForMelee => _minAlliesForMelee;
    public int MinAlliesForCapture => _minAlliesForCapture;


    [Header("GUI elements")]
    [SerializeField] protected SimpleHealthBar _healthBar;
    [SerializeField] protected SimpleHealthBar _ammoBar;
    [SerializeField] protected RawImage _status;
    [SerializeField] protected Animator _statusAnimator;
    [SerializeField] protected Animator _criticalHitAnimator;

    [Header("Active information")]
    // NPC active information
    [SerializeField] protected int _currentHealth;
    [SerializeField] protected int _currentAmmo;
    private int _currentHealthAnimation;
    private Coroutine _healthAnimationCoroutine;
    private Vector3 _endPath;
    public Animator CriticalHitAnimator => _criticalHitAnimator;

    public Vector3 EndPath {
        get => _endPath;
        set => _endPath = value;
    }

    // States
    [SerializeField] private State _currentState;
    private Capture _captureState;
    private Defend _defendState;
    private Escape _escapeState;
    private Heal _healState;
    private Idle _idleState;
    private MeleeAttack _meleeAttackState;
    private Patrol _patrolState;
    private RangedAttack _rangedAttackState;
    private Dead _deadState;
    private Reload _reloadState;
    private User _userState;
    private Roam _roamState;

    public State CurrentState => _currentState;
    public Capture CaptureState => _captureState;
    public Defend DefendState => _defendState;
    public Escape EscapeState => _escapeState;
    public Heal HealState => _healState;
    public Idle IdleState => _idleState;
    public MeleeAttack MeleeAttackState => _meleeAttackState;
    public Patrol PatrolState => _patrolState;
    public RangedAttack RangedAttackState => _rangedAttackState;
    public Dead DeadState => _deadState;
    public Reload ReloadState => _reloadState;
    public User UserState => _userState;
    public Roam RoamState => _roamState;
    public bool IsDead => _currentState == _deadState;

    // Other
    private Tile _lastValidTile;
    public Tile CurrentTile {
        get {
            var currentTile = _gridMap.GetTile(_agentNPC.Position);
            if (!currentTile.isWalkable) 
                return _lastValidTile;
            
            return currentTile;
        }
    }

    private Vector3 _startingPosition;

    void Start() {
        if (_pathfinding) {
            _pathfinding.Type = _unitType;
            _pathfinding.Team = _team;
        }
        _agentNPC = GetComponent<AgentNPC>();
        _simplePropagator = GetComponent<SimplePropagator>();
        Initialize();
    }
    
    
    void Update() {
        if (_currentState != null)
            _currentState.Execute(this);
        
        // Most likely not the best way to do this
        _agentNPC.MaxVelocity = _groupSpeed * _gridMap.GetTile(_agentNPC.Position).SpeedMultiplier(_pathfinding.Type);
        var currentTile =  _gridMap.GetTile(_agentNPC.Position);

        if (currentTile.isWalkable)
            _lastValidTile = currentTile;
    }

    protected void Initialize() {
        _currentState = null;
        _captureState = new Capture();
        _defendState = new Defend();
        _escapeState = new Escape();
        _healState = new Heal();
        _idleState = new Idle();
        _meleeAttackState = new MeleeAttack();
        _patrolState = new Patrol();
        _rangedAttackState = new RangedAttack();
        _deadState = new Dead();
        _reloadState = new Reload();
        _userState = new User();
        _roamState = new Roam();
        _currentHealth = _maxHealth;
        _currentHealthAnimation = _maxHealth;
        _currentAmmo = _ammo;
        _groupSpeed = _speed;
        _startingPosition = AgentNPC.Position;
        ChangeState(_idleState);
    }

    public void ChangeState(State newState, NPC objective = null) {

        if (_currentState != null && _currentState != newState)
            _currentState.Exit(this);

        newState.SetObjective(objective);

        if (_currentState != newState) {
            _currentState = newState;
            GUIManager.TriggerAnimation(_statusAnimator);
            _currentState.Enter(this);
        }
    }
    
    private IEnumerator AnimateHealthChange() {
        float currentAnimationVelocity = 0;
        float rotationVelocity = 0.1f;
        while (_currentHealthAnimation != _currentHealth) {

            _currentHealthAnimation = (int)Mathf.SmoothDamp(_currentHealthAnimation, _currentHealth,
                ref currentAnimationVelocity, rotationVelocity);
            _healthBar.UpdateBar(_currentHealthAnimation, _maxHealth);
            yield return null;
        }

        _healthAnimationCoroutine = null;
    }

    public void UpdateStateIcon() {
        GUIManager.UpdateStateIcon(_status, _currentState);
    }

    public void ToggleOffensiveMode() {
        _enemiesToRun = _initialEnemiesToRun + 1;
        _lowHealth = _initialLowHealth - 10;
        _healthy = _initialHealthy - 20;
        _maxEnemiesForMelee = _initialMaxEnemiesForMelee + 1;
        _minAlliesForCapture = 0;
        if (_minAlliesForMelee > 0)
            _minAlliesForMelee = _initialMinAlliesForMelee - 1;

        // Pathfinding
        _pathfinding.TerrainCostMultiplier = 1;
        _pathfinding.InfluenceCostMultiplier = 1;
        _pathfinding.VisibilityCostMultiplier = 2;
    }

    public void ToggleDefensiveMode() {
        _enemiesToRun = _initialEnemiesToRun - 1;
        _lowHealth = _initialLowHealth + 20;
        _healthy = _initialHealthy + 10;
        _minAlliesForCapture = _initialAlliesForCapture;
        if (_maxEnemiesForMelee > 0)
            _maxEnemiesForMelee = _initialMaxEnemiesForMelee - 1;
        _minAlliesForMelee = _initialMinAlliesForMelee + 1;

        // Pathfinding
        _pathfinding.TerrainCostMultiplier = 1;
        _pathfinding.InfluenceCostMultiplier = 2;
        _pathfinding.VisibilityCostMultiplier = 1;
    }

    public void ToggleTotalWar() {
        _enemiesToRun = _initialEnemiesToRun + 2;
        _lowHealth = _initialLowHealth - 20;
        _healthy = _initialHealthy - 30;
        _maxEnemiesForMelee = _initialMaxEnemiesForMelee + 2;
        _minAlliesForCapture = 0;
        if (_minAlliesForMelee > 0)
            _minAlliesForMelee = _initialMinAlliesForMelee - 1;

        // Pathfinding
        _pathfinding.TerrainCostMultiplier = 1;
        _pathfinding.InfluenceCostMultiplier = 0;
        _pathfinding.VisibilityCostMultiplier = 0;
    }

    public void AddToGroup(float speed, UnitType type) {
        _groupSpeed = speed;
        _pathfinding.Type = type;
    }

    public void RemoveFromGroup() {
        _groupSpeed = _speed;
        _pathfinding.Type = _unitType;
    }

    public void Restart() {
        Health = MaxHealth;
        CurrentAmmo = Ammo;
        RemoveFromGroup();
        Pathfinding.ClearPath();
        AgentNPC.Position = _startingPosition;
        ChangeState(_idleState);
    }
}