using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUICaller : MonoBehaviour {
    
    private NPC _npc;
    // Callable functions during animation events must be directly attached to the object holding the animator
    
    private void Awake() {
        _npc = GetComponentInParent<NPC>();
    }

    public void CallUpdateStateIcon() {
        _npc.UpdateStateIcon();
    }
}
