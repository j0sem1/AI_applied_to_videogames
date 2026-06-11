using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingText : MonoBehaviour {

    public float DestroyTime = 0.42f; // Should match the animation time
    public Vector3 Offset = new Vector3(0, 2, 1);
    public Vector3 RandomizeIntensity = new Vector3(0.75f, 0, 0.2f);
    
    // Start is called before the first frame update
    void Start() {
        // Place the numeric indicator in a random position for a limited time
        Destroy(gameObject, DestroyTime);
        transform.localPosition += Offset + new Vector3(Random.Range(-RandomizeIntensity.x, RandomizeIntensity.x), 
                                       Random.Range(-RandomizeIntensity.y, RandomizeIntensity.y),
                                       Random.Range(-RandomizeIntensity.z, RandomizeIntensity.z));
    }

}
