using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningDude : MonoBehaviour
{
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float T = Time.time;
        float DT = Time.deltaTime;
    }

    // FixedUpdate is called several times per frame
    void FixedUpdate() 
    {
        float FDT = Time.fixedDeltaTime;

        // most of the time (90%) you are dealing with positions, vectors, and quaternions
        // no need to use matrices unless you are using Gizmos to visualize things

        // rotates the character around its local upward axis

        // Space.Self => Local
        // Space.World => Global
        transform.Rotate(Vector3.up * 90F * FDT, Space.Self);

        transform.Translate(Vector3.forward * 32F * FDT, Space.World);
    }
}
