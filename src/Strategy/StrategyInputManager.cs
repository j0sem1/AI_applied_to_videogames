using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyInputManager : MonoBehaviour {
    
    public struct Unit {
        public NPC npc;
        public Outline outline;
    } 

    // Inspector elements
    [Header("Selection elements")]
    public NPC[] SelectableNPC;
    public Color SelectionColor;
    public Color SelectionBorderColor;
    public float BorderThickness;
    
    [Header("Group elements")]
    public Color UnitOutlineColor;        // Color of a unit that is selected individually
    public Color GroupOutlineColor;       // Color of a unit that belongs to a group
    public Color GroupUnitOutlineColor;   // Color of a selected unit that also belongs to a group
    
    [Header("Mode elements")]
    public RawImage RedOffensiveMode;
    public RawImage RedDefensiveMode;
    public RawImage BluOffensiveMode;
    public RawImage BluDefensiveMode;
    public RawImage TotalWarMode;

    private Animator _redOffensiveAnimator;
    private Animator _redDefensiveAnimator;
    private Animator _bluOffensiveAnimator;
    private Animator _bluDefensiveAnimator;
    private Animator _totalWarAnimator;

    // Private elements
    private Unit[] _selectableUnits;
    private List<int> _selectedUnits;
    private List<int> _highlightedGroupUnits;
    private RaycastHit _hit;
    private bool _isDragging = false;
    private Vector3 _clickPosition;
    private bool _multipleSelection = false;
    [SerializeField] private bool _redOffensive = false;
    [SerializeField] private bool _bluOffensive = false;
    [SerializeField] private bool _totalWarMode = false;

    private GameManager _gameManager;
    public GameManager GameManager {
        get => _gameManager;
        set => _gameManager = value;
    }

    // Start is called before the first frame update
    void Start() {
        _redOffensiveAnimator = RedOffensiveMode.GetComponent<Animator>();
        _redDefensiveAnimator = RedDefensiveMode.GetComponent<Animator>();
        _bluOffensiveAnimator = BluOffensiveMode.GetComponent<Animator>();
        _bluDefensiveAnimator = BluDefensiveMode.GetComponent<Animator>();
        _totalWarAnimator = TotalWarMode.GetComponent<Animator>();

        _totalWarMode = false;

        GUIManager.TriggerAnimation(_redDefensiveAnimator);
        GUIManager.TriggerAnimation(_bluDefensiveAnimator);
        _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Red);
        _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Blu);


        _selectedUnits = new List<int>();
        _highlightedGroupUnits = new List<int>();
        _selectableUnits = new Unit[SelectableNPC.Length];
        for (int i = 0; i < SelectableNPC.Length; i++) {
            _selectableUnits[i].npc = SelectableNPC[i];
            _selectableUnits[i].outline = SelectableNPC[i].gameObject.GetComponent<Outline>();
        }
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
    }

    private int TransformToSelectableUnit(Transform t) {
        var npc = t.GetComponent<NPC>();
        for (int i = 0; i < _selectableUnits.Length; i++) {
            if (npc.Equals(_selectableUnits[i].npc))
                return i;
        }

        return -1;
    }

    private void HandleInput() {
        _multipleSelection = Input.GetKey(KeyCode.LeftShift); // Left SHIFT for multiple selection
        
        // "g" to add multiple selected units to a group 
        if (Input.GetKeyDown(KeyCode.G) && _selectedUnits.Count > 1 && SelectedUnitsBelongToSameTeam()) {
            GroupManager.CreateGroup(_selectedUnits);
            float slowestSpeed = float.MaxValue;
            NPC.UnitType slowestType = NPC.UnitType.Heavy;
            foreach (var i in _selectedUnits) {
                _selectableUnits[i].outline.OutlineColor = GroupUnitOutlineColor;
                var npcSpeed = _selectableUnits[i].npc.Speed;
                if (npcSpeed < slowestSpeed) {
                    slowestSpeed = npcSpeed;
                    slowestType = _selectableUnits[i].npc.Type;
                }
            }

            foreach (var i in _selectedUnits) {
                _selectableUnits[i].npc.AddToGroup(slowestSpeed, slowestType);
            }
            return;
        }
        
        // "c" to clear selected units from all groups 
        if (Input.GetKeyDown(KeyCode.C) && _selectedUnits.Count > 0) {
            if (_selectedUnits.Count == 1) {
                if (GroupManager.RemoveMemberFromGroup(_selectedUnits[0])) {
                    _selectableUnits[_selectedUnits[0]].outline.OutlineColor = UnitOutlineColor;
                    foreach (int i in _highlightedGroupUnits) {
                        _selectableUnits[i].outline.enabled = false;
                    }
                    _highlightedGroupUnits.Clear();
                }
            }
            else {
                foreach (int i in _selectedUnits) {
                    _selectableUnits[i].outline.OutlineColor = UnitOutlineColor;
                    _selectableUnits[i].npc.RemoveFromGroup();
                    GroupManager.RemoveMemberFromGroup(i);
                }
            }
     
            return;
        }

        // Left click to select units
        if (Input.GetMouseButtonDown(0)) {
            // Update elements
            _clickPosition = Input.mousePosition;
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out _hit)) {
                
                // If so, add it to the selected units list
                if (_hit.transform.CompareTag("Unit")) {
                    SelectUnit(TransformToSelectableUnit(_hit.transform), _multipleSelection);
                } else {
                    // Otherwise, begin drag selection
                    _isDragging = true;
                }
            }
        } else if (Input.GetMouseButtonUp(0) && _isDragging) {
            // Clear the previous selection if SHIFT isn't being pressed down
            if (!_multipleSelection) {
                ClearSelectedUnits();
            }

            // Check if any of our selectable units is within the rectangle selection
            for (int i = 0; i < _selectableUnits.Length; i++) {
                // Ignore disabled objects
                if (!_selectableUnits[i].npc.gameObject.activeSelf)
                    continue;
                if (IsWithinSelectionBounds(_selectableUnits[i].npc.transform))
                    SelectUnit(i, true);
            }
            _isDragging = false; 
        }
    
        // Right click while having selected units to make them go the right click position
        if (Input.GetMouseButtonDown(1) && _selectedUnits.Count == 1) {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out _hit, 1000)) {

                string tag = _hit.collider.tag;
                if (tag == "Road" || tag == "Grassland" || tag == "Forest" || tag == "RedCapturePoint" ||
                    tag == "BluCapturePoint" || tag == "RedBase" || tag == "BluBase") {

                    // Ignore the Y Axis
                    Vector3 twoAxis = _hit.point;
                    twoAxis.y = 0.1f;

                    var npc = _selectableUnits[_selectedUnits[0]].npc;
                    if (npc.Team == NPC.UnitTeam.Red && tag == "BluBase")
                        return;

                    if (npc.Team == NPC.UnitTeam.Blu && tag == "RedBase")
                        return;

                    npc.Pathfinding.ClearPath();
                    npc.Pathfinding.FindPathToPosition(npc.CurrentTile.worldPosition, twoAxis);
                    npc.EndPath = twoAxis;
                    npc.ChangeState(npc.UserState);
                }
            }
        }

        // "m" to mute the music
        if (Input.GetKeyDown(KeyCode.M)) {
            _gameManager.MuteUnmuteMusic();
        }
    }

    private void SelectUnit(int i, bool multipleSelection) {
        if (i == -1) {
            Debug.LogWarning("That shouldn't happen");
            return;
        }
        // Reset the current selection if SHIFT isn't being pressed down and something is already selected
        if (!multipleSelection && _selectedUnits.Count > 0) {
            ClearSelectedUnits();
        }

        if (_selectedUnits.Contains(i))
            return;

        var unit = _selectableUnits[i];

        if (unit.npc.IsDead)
            return;
        
        // Check if the unit belongs to a group
        var group = GroupManager.GetGroup(i);

        if (group != null) {
            unit.outline.OutlineColor = GroupUnitOutlineColor;
            // Stop highlighting other group members if more than one unit is selected
            if (_selectedUnits.Count == 0) {
                foreach (int j in group) {
                    if (i == j)
                        continue;
                    _highlightedGroupUnits.Add(j);
                    _selectableUnits[j].outline.OutlineColor = GroupOutlineColor;
                    _selectableUnits[j].outline.enabled = true;
                }
            } else if (_selectedUnits.Count == 1) {
                foreach (int j in _highlightedGroupUnits) {
                    _selectableUnits[j].outline.enabled = false;
                }
                _highlightedGroupUnits.Clear();
            }
        }
        else {
            unit.outline.OutlineColor = UnitOutlineColor;
        }

        unit.outline.enabled = true;


        _selectedUnits.Add(i);
    }

    private void ClearSelectedUnits() {
        foreach (int i in _selectedUnits) {
            _selectableUnits[i].outline.enabled = false;
        }
        _selectedUnits.Clear();
        foreach (int i in _highlightedGroupUnits) {
            _selectableUnits[i].outline.enabled = false;
        }
        _highlightedGroupUnits.Clear();
    }

    private void OnGUI() {
        // Draw the rectangle on dragging
        if (_isDragging) {
            Rect rect = ScreenHelper.GetScreenRect(_clickPosition, Input.mousePosition);
            ScreenHelper.DrawScreenRect(rect, SelectionColor);
            ScreenHelper.DrawScreenRectBorder(rect, BorderThickness, SelectionBorderColor);
        }
    }

    // Helper function to check whether a transform is within our rectangle selection or not
    private bool IsWithinSelectionBounds(Transform transform) {
        if (!_isDragging)
            return false;
        
        Camera cam = Camera.main;
        Bounds bounds = ScreenHelper.GetViewportBounds(cam, _clickPosition, Input.mousePosition);
        return bounds.Contains(cam.WorldToViewportPoint(transform.position));
    }

    // Using bools so this method can be called OnClick()
    public void ToggleRedMode(bool offensive) {
        if (_totalWarMode) {

            _totalWarMode = false;

            if (_bluOffensive) {
                _gameManager.ToggleOffensiveMode(NPC.UnitTeam.Blu);
                GUIManager.TriggerAnimation(_bluOffensiveAnimator);
                
            } else {
                _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Blu);
                GUIManager.TriggerAnimation(_bluDefensiveAnimator);
            }
                
            GUIManager.TriggerAnimation(_totalWarAnimator);

            _redOffensive = offensive;

            if (_redOffensive) {
                GUIManager.TriggerAnimation(_redOffensiveAnimator);
                _gameManager.ToggleOffensiveMode(NPC.UnitTeam.Red);
                
            } else {
                _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Red);
                GUIManager.TriggerAnimation(_redDefensiveAnimator);
                
            }
            
        } else if (_redOffensive != offensive) {
            _redOffensive = offensive;

            GUIManager.TriggerAnimation(_redOffensiveAnimator);
            GUIManager.TriggerAnimation(_redDefensiveAnimator);

            if (_redOffensive) {
                _gameManager.ToggleOffensiveMode(NPC.UnitTeam.Red);
            } else {
                _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Red);
                
            }
                
        }
    }
    
    public void ToggleBluMode(bool offensive) {
        if (_totalWarMode) {

            _totalWarMode = false;
            if (_redOffensive) {
                _gameManager.ToggleOffensiveMode(NPC.UnitTeam.Red);
                GUIManager.TriggerAnimation(_redOffensiveAnimator);
                
            } else {
                _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Red);
                GUIManager.TriggerAnimation(_redDefensiveAnimator);
                
            }

            GUIManager.TriggerAnimation(_totalWarAnimator);

            _bluOffensive = offensive;

            if (_bluOffensive) {
                _gameManager.ToggleOffensiveMode(NPC.UnitTeam.Blu);
                GUIManager.TriggerAnimation(_bluOffensiveAnimator);
                
            } else {
                _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Blu);
                GUIManager.TriggerAnimation(_bluDefensiveAnimator);
                
            }

        } else if (_bluOffensive != offensive) {
            _bluOffensive = offensive;

            GUIManager.TriggerAnimation(_bluOffensiveAnimator);
            GUIManager.TriggerAnimation(_bluDefensiveAnimator);

            if (_bluOffensive) {
                _gameManager.ToggleOffensiveMode(NPC.UnitTeam.Blu);
                
            } else {
                _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Blu);
                
            }
                
        }
    }

    private bool SelectedUnitsBelongToSameTeam() {
        var team = NPC.UnitTeam.Red;
        bool first = true;
        foreach (int i in _selectedUnits) {
            if (first) {
                team = _selectableUnits[i].npc.Team;
                first = false;
            }
            else {
                if (_selectableUnits[i].npc.Team != team)
                    return false;
            }
        }

        return true;
    }

    public void EnableTotalWarMode() {
        if (_totalWarMode)
            return;
        
        _totalWarMode = true;
        GUIManager.TriggerAnimation(_totalWarAnimator);

        if (_redOffensive)
            GUIManager.TriggerAnimation(_redOffensiveAnimator);
        else
            GUIManager.TriggerAnimation(_redDefensiveAnimator);
        
        if (_bluOffensive)
            GUIManager.TriggerAnimation(_bluOffensiveAnimator);
        else
            GUIManager.TriggerAnimation(_bluDefensiveAnimator);

        _gameManager.EnableTotalWar();
    }

    public void Restart() {
        
        // Clear input related elements
        ClearSelectedUnits();
        GroupManager.ClearAllGroups();

        if (_totalWarMode) {
            _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Blu);
            _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Red);
            GUIManager.TriggerAnimation(_totalWarAnimator);
            GUIManager.TriggerAnimation(_redDefensiveAnimator);
            GUIManager.TriggerAnimation(_bluDefensiveAnimator);
            _bluOffensive = false;
            _redOffensive = false;
            _totalWarMode = false;
        }
        else {
            if (_redOffensive) {
                _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Red);
                GUIManager.TriggerAnimation(_redDefensiveAnimator);
                GUIManager.TriggerAnimation(_redOffensiveAnimator);
                _redOffensive = false;
            } 
            if (_bluOffensive) {
                _gameManager.ToggleDefensiveMode(NPC.UnitTeam.Blu);
                GUIManager.TriggerAnimation(_bluDefensiveAnimator);
                GUIManager.TriggerAnimation(_bluOffensiveAnimator);
                _bluOffensive = false;
            }
        }
        
        // Clear game flow elements
        CombatManager.Restart();
        _gameManager.Restart();
    }

}
