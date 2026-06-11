using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class assign the targets of the scripts from every object of the group
public class GroupController : MonoBehaviour
{

    public Transform[] objects;

    // Assing targets to the Alignment script
    private void PutAlignment(Alignment script, int excluded)
    {
        for(int i = 0; i < objects.Length; i++)
        {
            if (i != excluded)
                script.Targets.Add(objects[i].GetComponent<Agent>());
        }
    }

    // Assing targets to the Cohesion script
    private void PutCohesion(Cohesion script, int excluded)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (i != excluded)
                script.Targets.Add(objects[i].GetComponent<Agent>());
        }
    }

    // Assing targets to the Separation script
    private void PutSeparation(Separation script, int excluded)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (i != excluded)
                script.Targets.Add(objects[i].GetComponent<Agent>());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        objects = new Transform[transform.childCount];

        int i = 0;

        foreach (Transform t in transform)
        {
            objects[i++] = t;
        }

        // Update Alignment, Cohesion and Separation of every child
        for (i = 0; i < objects.Length; i++)
        {
            // Assign all the objects to the Alignment script excluding the actual object
            PutAlignment(objects[i].GetComponent<Alignment>(), i);

            // Assign all the objects to the Cohesion script excluding the actual object
            PutCohesion(objects[i].GetComponent<Cohesion>(), i);

            // Assign all the objects to the Separation script excluding the actual object
            PutSeparation(objects[i].GetComponent<Separation>(), i);
        }
    }
}