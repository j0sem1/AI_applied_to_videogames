using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GUIKillFeed : MonoBehaviour {
    // Defines the whole kill feed, utilizing individual GUIKills
    private float _timeOnScreen = 5f;

    private class KillStatus {
        public int position;
        public float timer;
    }

    [SerializeField] private RectTransform _self;
    [SerializeField] private GUIKill[] _killFeed;    // All four available kill "slots"
    [SerializeField] private Vector3[] _killPosition;    // and their position
    private LinkedList<KillStatus> _queue; // Currently used kill "slots"

    void Awake() {
        CombatManager.GuiKillFeed = this;
        _queue = new LinkedList<KillStatus>();
        _killPosition = new Vector3[_killFeed.Length];
        for (int i = 0; i < _killFeed.Length; i++) {
            _killFeed[i].gameObject.SetActive(false);
            _killPosition[i] = _killFeed[i].transform.localPosition;
        }
    }

    public void AddKill(NPC killer, NPC victim, bool melee, bool crit) {
        KillStatus status = new KillStatus();
        // All kill feed positions are occupied, clear the oldest one
        // Same instructions apply if no kill feed positions are occupied
        if (_queue.Count == _killFeed.Length || _queue.Count == 0) {
            var old = _queue.First;
            if (old != null) {
                _killFeed[old.Value.position].gameObject.SetActive(false);
                status.position = old.Value.position;
                _queue.RemoveFirst();
            }
            else {
                status.position = 0;
            }
            
            _killFeed[status.position].Initialize(killer, victim, melee, crit);
            _killFeed[status.position].gameObject.SetActive(true);
            status.timer = _timeOnScreen;
            _queue.AddLast(status);
        }
        else {
            // Find the lowest value available
            int lowest = 0;
            for (int i = 0; i < _killFeed.Length; i++) {
                if (!_killFeed[i].gameObject.activeSelf) {
                    lowest = i;
                    break;
                }
            }
            status.position = lowest;
            _killFeed[lowest].Initialize(killer, victim, melee, crit);
            _killFeed[lowest].gameObject.SetActive(true);
            status.timer = _timeOnScreen;
            _queue.AddLast(status);
            
        }
        UpdatePositions();
    }

    private void UpdatePositions() {
        // Update the occupied kill "slots" depending on their position in the queue
        int i = 0;
        foreach (var status in _queue) {
            var position = _killPosition[i];
            position.x = _self.rect.width / 2 - _killFeed[status.position].Width / 2;
            _killFeed[status.position].transform.localPosition = position;
            i++;
        }
    }

    private void Update() {
        // Decrease the remaining time of all occupied kill "slots"
        bool update = false;
        var node = _queue.First;
        while (node != null) {
            node.Value.timer -= Time.deltaTime;
            var next = node.Next;
            if (node.Value.timer < 0f) {
                _killFeed[node.Value.position].gameObject.SetActive(false);
                _queue.Remove(node);
                update = true;
            }
            node = next;
        }
        
        // If any is removed, update the position of the rest
        if (update) 
            UpdatePositions();
    }

    public void Restart() {
        // Clear all slots
        foreach (var node in _queue) {
            _killFeed[node.position].gameObject.SetActive(false);
        }
        _queue.Clear();
    }
}