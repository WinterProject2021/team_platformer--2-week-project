using com.cozyhome.Actors;
using com.cozyhome.Timers;
using com.cozyhome.Vectors;
using UnityEngine;


public class JumpState : ActorState
{
    [SerializeField] private float jh = 1;

    public override void Enter(ActorState prev) {
        ActorHeader.Actor Actor = Machine.GetActor;

        Vector3 Velocity = Actor.velocity;
        Vector3 XZ = Velocity - Vector3.up * Vector3.Dot(Velocity, Vector3.up);
        Velocity   = XZ + Vector3.up * Mathf.Sqrt(2F * Physics.gravity.magnitude * jh);

        if(Actor.Ground.stable && Actor.Ground.snapped) {
            Rigidbody r = Actor.Ground.collider.attachedRigidbody;
            if(r) {
                // Velocity += r.Force * 10F; 
                Velocity += r.GetPointVelocity(Actor.position) * 2.0F;
                //Debug.Log(r.velocity);
            }
        }

        Actor.SetVelocity(Velocity);
    }

    public override void Exit(ActorState next)
    { }

    public override void OnGroundHit(ActorHeader.GroundHit ground, ActorHeader.GroundHit lastground, LayerMask layermask)
    { }

    public override void OnTraceHit(ActorHeader.TraceHitType tracetype, RaycastHit trace, Vector3 position, Vector3 velocity)
    { }

    public override void OnTriggerHit(ActorHeader.TriggerHitType triggertype, Collider trigger)
    { }

    public override void Tick(float fdt) { 
        ActorHeader.Actor Actor = Machine.GetActor;
        Vector3 Velocity = Actor.velocity;

        if(DetermineTransitions())
            return;

        Velocity += Physics.gravity * fdt;
        
        Actor.SetVelocity(Velocity);
    }

    protected override void OnStateInitialize()
    { }

    bool DetermineTransitions() {
        ActorHeader.Actor Actor = Machine.GetActor;
        Vector3 Velocity = Actor.velocity;
        
        if(Machine.GetPlayerInput.GetXTrigger) {
            bool success = Machine.GetFSM.TrySwitchState( (ActorState next) => {
                return ((SwingState) next).AttemptGrapple();
            }, "Swing");

            // if(success) {
            //     // i mean we don't really need to do much here
            // }

            return success;
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
}