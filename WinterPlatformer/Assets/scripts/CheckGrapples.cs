using System;
using System.Collections;
using System.Collections.Generic;
using com.cozyhome.Vectors;
using UnityEngine;

public class CheckGrapples : MonoBehaviour
{
    [SerializeField] private Transform View;
    [SerializeField] private Transform Actor;

    [System.Serializable] struct GrappleReport {
        public int instance;
        public int index;
        public Collider collider;

        public GrappleReport(int instance, int index, Collider collider) {
            this.index    = index;
            this.instance = instance;
            this.collider = collider;
        }
    }

    private Dictionary<int, GrappleReport> DetectedGrapples;
    [SerializeField] private GrappleReport[] Grapples;
    private int count;
    private int alloc = 20;

    void Start() {
        DetectedGrapples = new Dictionary<int, GrappleReport>();
        Grapples         = new GrappleReport[alloc];
        count = 0;

        for(int i = 0;i < alloc;i++)
            Grapples[i] = new GrappleReport(-1, -1, null);  
    }

    void OnTriggerEnter(Collider other) { 
        if(other.tag != "Grapple")
            return;

        int instid = other.GetInstanceID();
        
        if(!DetectedGrapples.ContainsKey(instid )) {
            GrappleReport grp = new GrappleReport(-1, -1, null);
            grp.instance = instid;
            grp.collider = other;
            grp.index = count++;
            count %= alloc;

            DetectedGrapples.Add(other.GetInstanceID(), grp);
            Grapples[grp.index] = grp;

        }
    }

    void OnTriggerExit(Collider other) {
        if(other.tag != "Grapple")
            return;
        
        int instid = other.GetInstanceID();
        
        if(DetectedGrapples.ContainsKey(instid)) {
            int idx = DetectedGrapples[instid].index;
            Grapples[idx].index = -1; // clear
            Grapples[idx].instance = -1; // clear
            Grapples[idx].collider = null;

            DetectedGrapples.Remove(instid);
            
        }
    }

    public bool TryGrappleReport(out Transform grapple) {
        var t = FindNearestDirection();
        grapple = t;

        return t == null ? false : true;
    }

    void Update() {
        Transform t = FindNearestDirection();
        if(t != null)
            Debug.DrawLine(Actor.position, t.position, Color.red);
    }

    Transform FindNearestDirection() {
        int inst_id = -1;
        float min_d = 0F;

        for(int i = 0;i < alloc;i++) {
            if(Grapples[i].index == -1)
                continue;
            else {
                Debug.DrawLine(Actor.position, Grapples[i].collider.transform.position, Color.cyan);
                //     // Grapples[i].collider.transform.position - View.position;
                float toi = VectorHeader.LinePlaneIntersection(
                    (Grapples[i].collider.transform.position, -Vector3.up),
                    (View.position, View.up));
                

                Vector3 d = Grapples[i].collider.transform.position - Vector3.up * toi - View.position;
                float mag = d.magnitude;

                Debug.DrawRay(View.position, d, Color.magenta);
                float cur_d = VectorHeader.Dot(View.forward, d / mag);
                // float cur_d = -VectorHeader.Dot(Vector3.Cross(View.forward, d), Vector3.up);

                if(cur_d > min_d) {
                    min_d   = cur_d;
                    inst_id = Grapples[i].instance; 
                }
            }
        }

        if(inst_id != -1)
            Debug.DrawLine(Actor.position, DetectedGrapples[inst_id].collider.transform.position, Color.red);


        return inst_id == -1 ? null : DetectedGrapples[inst_id].collider.transform;
    }
}
