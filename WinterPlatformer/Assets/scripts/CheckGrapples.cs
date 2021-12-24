using System;
using System.Collections;
using System.Collections.Generic;
using com.cozyhome.Vectors;
using UnityEngine;

public class CheckGrapples : MonoBehaviour
{
    [SerializeField] private Transform View;
    [SerializeField] private Transform Actor;

    struct GrappleReport {
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
    private GrappleReport[] Grapples;
    private int count;

    void Start() {
        DetectedGrapples = new Dictionary<int, GrappleReport>();
        Grapples         = new GrappleReport[10];
        count = 0;

        for(int i = 0;i < 10;i++)
            Grapples[i] = new GrappleReport(-1, -1, null);  
    }

    void OnTriggerEnter(Collider other) { 
        int instid = other.GetInstanceID();
        
        if(!DetectedGrapples.ContainsKey(instid )) {
            GrappleReport grp = new GrappleReport(-1, -1, null);
            grp.instance = instid;
            grp.collider = other;
            grp.index = count++;
            count %= 10;

            DetectedGrapples.Add(other.GetInstanceID(), grp);
            Grapples[grp.index] = grp;

        }
    }

    void OnTriggerExit(Collider other) {
        int instid = other.GetInstanceID();
        
        if(DetectedGrapples.ContainsKey(instid)) {
            int idx = DetectedGrapples[instid].index;
            Grapples[idx].index = -1; // clear
            Grapples[idx].instance = -1; // clear
            count--;

            DetectedGrapples.Remove(instid);
            
        }
    }

    public bool TryGrappleReport(out Transform grapple) {
        var t = FindNearestDirection();
        grapple = t;

        return t == null ? false : true;
    }

    // void Update() {
    //     FindNearestDirection();
    // }

    Transform FindNearestDirection() {
        int inst_id = -1;
        float min_d = 0F;

        for(int i = 0;i < 10;i++) {
            if(Grapples[i].index == -1)
                continue;
            else {
                Vector3 d = Grapples[i].collider.transform.position - View.position;
                float mag = d.sqrMagnitude;
                mag = 1 / mag;

                float cur_d = VectorHeader.Dot(View.forward, (d).normalized);
            
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
