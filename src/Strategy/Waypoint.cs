using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{

    [SerializeField] protected SimpleHealthBar _healthBar = null;

    public enum WaypointType {
        RedBase,
        BluBase,
        Cover,
        RedCheckpoint,
        BluCheckpoint,
        Roam
    }

    [SerializeField] private WaypointType _waypointType;
    public WaypointType Type => _waypointType;

    [SerializeField] private Vector3 _position;
    public Vector3 Position {
        get => _position;
        set => _position = value;
    }
    [SerializeField] private float _capturePercentage;
    public float CapturePercentage {
        get => _capturePercentage;
        set => _capturePercentage = value;
    }
    [SerializeField] private Transform[] _positions;
    public Transform[] Positions => _positions;

    void Start() {
        _position = transform.position;
        if (_waypointType == WaypointType.RedCheckpoint || _waypointType == WaypointType.BluCheckpoint)
            _capturePercentage = 0;
    }

    void Update() {
        if (_healthBar != null)
            _healthBar.UpdateBar(_capturePercentage, 100);
    }
}
