using UnityEngine;
using com.cozyhome.Actors;
using com.cozyhome.Timers;
using com.cozyhome.Vectors;
using System;

public class SwingState : ActorState
{
    [SerializeField] private AnimationCurve t_curve;
    [SerializeField] private CheckGrapples grapples;
    [SerializeField] private float t_multipl = .125F;
    [SerializeField] private float mint_length = 5F;

    [SerializeField] private TimerHeader.DeltaTimer Timer;

    private Transform grapple_t;
    private float maxt_length;
    private float t_length;

    public void Assign(Transform grapple_t) {
        this.grapple_t = grapple_t;
    }

    public override void Enter(ActorState prev) { 
        Timer.Reset();

        ActorHeader.Actor Actor = Machine.GetActor;
        Vector3 Tension = (Actor.position - grapple_t.position);
        //Vector3 Velocity = Vector3.zero; //VectorHeader.ClipVector(Actor.velocity, Vector3.Cross(Actor.velocity, Tension).normalized);// Vector3.zero;//Actor.velocity;
        // generate a v vector that exists on the halfplane containing T;

        Vector3 P = Vector3.Cross(Tension, Vector3.up);
        P.Normalize();

        Vector3 Velocity = VectorHeader.ClipVector(Actor.velocity, P);

        swingvel = Velocity;

        t_length = maxt_length = (grapple_t.position - Machine.GetActor.position).magnitude;
    }

    public override void Exit(ActorState next) { }

    public override void OnGroundHit(ActorHeader.GroundHit ground, ActorHeader.GroundHit lastground, LayerMask layermask) { }
    public override void OnTraceHit(ActorHeader.TraceHitType tracetype, RaycastHit trace, Vector3 position, Vector3 velocity) {
        // this.swingvel =  -Machine.GetActor.velocity;
        // Machine.GetActor.SetVelocity( this.swingvel );
        // this.swingvel *= Time.fixedDeltaTime;
    }

    public override void OnTriggerHit(ActorHeader.TriggerHitType triggertype, Collider trigger) { }

    Vector3 swingvel = Vector3.zero;

    public override void Tick(float fdt) {
        ActorHeader.Actor Actor = Machine.GetActor;
        Vector3 Velocity = Vector3.zero;

        if(DetermineTransitions())
            return;

        swingvel += t_multipl * Physics.gravity * fdt;
        Vector3 tmpvel = swingvel * fdt;

        Swing(ref tmpvel, ref Velocity, fdt); 
        Vector3 ta = (Actor.position - grapple_t.position);
        Actor.SetVelocity(Velocity / fdt/* + Vector3.Cross(ta, Vector3.up) * 20F * Velocity.magnitude*/);
        swingvel = tmpvel / fdt;

        Timer.Accumulate(fdt);
    }

    protected override void OnStateInitialize() {
        
    }

    bool DetermineTransitions() {
        ActorHeader.Actor Actor = Machine.GetActor;
        Vector3 Velocity = Actor.velocity;
        
        if(Machine.GetPlayerInput.GetXTrigger) {
            Machine.GetFSM.SwitchState("Fall");

            return true;
        }

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
        float m = ta.sqrMagnitude;
        float t = t_curve.Evaluate(Timer.NormalizedElapsed);
        float l = maxt_length * (1 - t) + mint_length * t;
        t_length = Mathf.MoveTowards(t_length, l, 20F * fdt);

        // get rotation basis and rotate vel based on its magnitude
        // Vector3 Move = ActorStateHeader.ComputeMoveVector(Machine.GetPlayerInput.GetRawMove, 
        //     Machine.GetCameraView.rotation, 
        //     Vector3.up
        // );

        // get difference if larger then we are in need of fixing
        if(m > t_length * t_length) {
            vel = grapple_t.position + ta * (t_length / Mathf.Sqrt(m)) - Actor.position;
            Vector3 tan = ta.normalized;
            trace_v = VectorHeader.ClipVector(trace_v, tan); //.normalized * old_v;
            Machine.GetModelView.rotation = Quaternion.LookRotation(trace_v, -tan);
        }
        else
            vel = trace_v;
    }
}