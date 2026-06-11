using System.Collections.Generic;
using Steering.Pathfinding;
using UnityEngine;

public class InputManager : MonoBehaviour {

    private struct Unit {
        public Agent agent;
        public Outline outline;
        public Seek seek;
        public Arrive arrive;
        public PathFinding pathFinding;
    }

    // Inspector elements
    public List<Transform> SelectableUnits;
    public Color SelectionColor;
    public Color SelectionBorderColor;
    public float BorderThickness;
    public Collider Floor;
    
    // Private elements
    private List<Unit> _selectedUnits;
    private RaycastHit _hit;
    private bool _isDragging = false;
    private Vector3 _clickPosition;
    private bool _multipleSelection = false;

    // Start is called before the first frame update
    void Start() {
        _selectedUnits = new List<Unit>();
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
    }

    private void HandleInput() {
        _multipleSelection = Input.GetKey(KeyCode.LeftShift); // Left SHIFT for multiple selection

        // Left click to select units
        if (Input.GetMouseButtonDown(0)) {

            // Update elements
            _clickPosition = Input.mousePosition;
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out _hit)) {
                //Debug.Log("Element clicked: " + _hit.transform);
                //Debug.Log("Position clicked on scene: " + _hit.point);

                // If so, add it to the selected units list
                if (_hit.transform.CompareTag("Unit")) {
                    SelectUnit(_hit.transform, _multipleSelection);
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
            foreach (Transform unit in SelectableUnits) {
                // Ignore disabled objects
                if (!unit.gameObject.activeSelf)
                    continue;
                
                if (IsWithinSelectionBounds(unit))
                    SelectUnit(unit, true);
            }
            _isDragging = false; 
        }
    
        // Right click while having selected units to make them go the right click position
        if (Input.GetMouseButtonDown(1) && _selectedUnits.Count > 0) {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Floor.Raycast(cameraRay, out _hit, 1000)) {

                // Ignore the Y Axis
                Vector3 twoAxis = _hit.point;
                twoAxis.y = 0.5f;

                foreach (Unit unit in _selectedUnits) {
                    if (unit.seek != null) {
                        unit.seek.Target = twoAxis;
                    }
                    if (unit.arrive != null) {
                        unit.arrive.Target = twoAxis;
                    }
                    if (unit.pathFinding != null) {
                        unit.pathFinding.FindPathToPosition(unit.agent.Position, twoAxis);
                    }
                }
            }
        }
    }

    private void SelectUnit(Transform target, bool multipleSelection) {
        // Reset the current selection if SHIFT isn't being pressed down and something is already selected
        if (!multipleSelection && _selectedUnits.Count > 0) {
            ClearSelectedUnits();
        }

        // Enable the outline highlight
        Outline outline = target.GetComponent<Outline>();
        if (outline.enabled)
            return; // Unit already selected
        outline.enabled = true;

        // Very inefficient but it is only used for the demo scenes
        Unit unit = new Unit();
        unit.outline = outline;
        unit.seek = target.GetComponent<Seek>();
        unit.arrive = target.GetComponent<Arrive>();
        unit.pathFinding = target.GetComponent<PathFinding>();
        unit.agent = target.GetComponent<Agent>();

        _selectedUnits.Add(unit);
    } 

    private void ClearSelectedUnits() {
        foreach (Unit unit in _selectedUnits) {
            unit.outline.enabled = false;
        }
        _selectedUnits.Clear();
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
}
