using System.Collections;
using System.Collections.Generic;
using com.cozyhome.Vectors;
using UnityEngine;

public class SwingerScript : MonoBehaviour
{
    
    [SerializeField] private Transform tetherpoint;
    private float tlength = 5F; 

    // Start is called before the first frame update
    void Start()
    {
        tlength = (tetherpoint.position - transform.position).magnitude; 
    }

    [SerializeField] Vector3 velocity;

    // Update is called once per frame
    void FixedUpdate() {
        float fdt = Time.fixedDeltaTime;
        Swing(fdt);
    }

    void Swing(float fdt) {
        
        // step 1:
            // integrate velocity and position
            // check to see if position is out of bounds
            // if so: set velocity to the difference vector to point on circle boundary ( revert position)

        velocity += Physics.gravity * fdt;
        Vector3 nextposition = transform.position + velocity;

        Vector3 ta = (nextposition - tetherpoint.position);
        float m = ta.magnitude;

        // get difference if larger then we are in need of fixing
        if(m > tlength) {
            nextposition = tetherpoint.position + ta.normalized * (tlength);
            velocity = VectorHeader.ClipVector(velocity, ta.normalized);
            transform.rotation = Quaternion.LookRotation(velocity, transform.up);
        }

        transform.position = nextposition;

    }
}
