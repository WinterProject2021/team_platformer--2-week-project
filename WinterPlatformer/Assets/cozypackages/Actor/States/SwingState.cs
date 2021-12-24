using UnityEngine;
using com.cozyhome.Actors;
using com.cozyhome.Timers;
using com.cozyhome.Vectors;
using System;

public class SwingState : ActorState
{
    [SerializeField] private float t_multipl = .125F;
    [SerializeField] private float t_length = 5F;
    [SerializeField] private CheckGrapples grapples;

    private Transform grapple_t;

    public void Assign(Transform grapple_t) {
        this.grapple_t = grapple_t;
    }

    public override void Enter(ActorState prev) { 
        
        ActorHeader.Actor Actor = Machine.GetActor;
        Vector3 Velocity = Actor.velocity;

        swingvel = Velocity;
    }

    public override void Exit(ActorState next) { }

    public override void OnGroundHit(ActorHeader.GroundHit ground, ActorHeader.GroundHit lastground, LayerMask layermask) { }
    public override void OnTraceHit(ActorHeader.TraceHitType tracetype, RaycastHit trace, Vector3 position, Vector3 velocity) { }
    public override void OnTriggerHit(ActorHeader.TriggerHitType triggertype, Collider trigger) { }

    Vector3 swingvel = Vector3.zero;

    public override void Tick(float fdt) {
        ActorHeader.Actor Actor = Machine.GetActor;
        Vector3 Velocity = Vector3.zero;

        if(DetermineTransitions())
            return;

        swingvel += t_multipl * Physics.gravity;
        Vector3 tmpvel = swingvel * fdt;

        Swing(ref tmpvel, ref Velocity, fdt); 
        Actor.SetVelocity(Velocity / fdt);
    
        swingvel = tmpvel / fdt;
    }

    protected override void OnStateInitialize() {
        
    }

    bool DetermineTransitions() {
        ActorHeader.Actor Actor = Machine.GetActor;
        Vector3 Velocity = Actor.velocity;
        
        // that long mathf.abs is essentially: V ->  <- N. When v is petruding into the boundary of N. 
        if(Actor.Ground.stable && Mathf.Abs(VectorHeader.Dot(Velocity, Actor.Ground.normal)) <= 0.1F) {
            Machine.GetFSM.SwitchState((ActorState next) => { 
                Actor.SetSnapEnabled(true);
            }, "Ground");

            return true;
        }

        return false;
    }

    public bool AttemptGrapple() {
        bool success = grapples.TryGrappleReport(out Transform grapple_t);
        if(success) {
            Assign(grapple_t);
            return true;
        }
        else
            return false;
    }

    void Swing(ref Vector3 trace_v, ref Vector3 vel, float fdt) {
        ActorHeader.Actor Actor = Machine.GetActor;

        // step 1:
            // integrate velocity and position
            // check to see if position is out of bounds
            // if so: set velocity to the difference vector to point on circle boundary ( revert position)
        Vector3 nextposition = Actor.position + trace_v;

        Vector3 ta = (nextposition - grapple_t.position);
        float m = ta.magnitude;

        // get difference if larger then we are in need of fixing
        if(m > t_length) {
            vel = grapple_t.position + ta.normalized * (t_length) - Actor.position;
            trace_v = VectorHeader.ClipVector(trace_v, ta.normalized);

            Machine.GetModelView.rotation = Quaternion.LookRotation(trace_v, transform.up);
        }
        else
            vel = trace_v;
    }
}