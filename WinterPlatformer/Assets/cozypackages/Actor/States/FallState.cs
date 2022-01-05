using UnityEngine;
using com.cozyhome.Actors;
using com.cozyhome.Timers;
using com.cozyhome.Vectors;

public class FallState : ActorState
{
    bool _onSwingEnter;

    public override void Enter(ActorState prev) { 

        _onSwingEnter = prev.GetKey == "Swing";

        ActorHeader.Actor Actor = Machine.GetActor;
        Actor.SetGroundTraceType(ActorHeader.GroundTraceType.Assigned);
        Actor.SetGroundTraceDir(-Vector3.up);
    }

    public override void Exit(ActorState next) { 
        ActorHeader.Actor Actor = Machine.GetActor;
        Actor.SetGroundTraceType(ActorHeader.GroundTraceType.Default);
    
        _onSwingEnter = false;
    }

    public override void OnGroundHit(ActorHeader.GroundHit ground, ActorHeader.GroundHit lastground, LayerMask layermask) { }
    public override void OnTraceHit(ActorHeader.TraceHitType tracetype, RaycastHit trace, Vector3 position, Vector3 velocity) { }
    public override void OnTriggerHit(ActorHeader.TriggerHitType triggertype, Collider trigger) { }

    public override void Tick(float fdt) {
        ActorHeader.Actor Actor = Machine.GetActor;
        Vector3 Velocity = Actor.velocity;

        if(DetermineTransitions())
            return;

        Velocity += Physics.gravity * fdt;
        Actor.SetVelocity(Velocity);

        float d = VectorHeader.Dot(Velocity.normalized, Vector3.up); 
        if(_onSwingEnter && d >= -.9F) {
            Actor.orientation = Quaternion.Slerp(
                Actor.orientation,
                Quaternion.LookRotation(
                    Velocity,//VectorHeader.ClipVector(Actor.orientation * Vector3.forward, Vector3.up).normalized,
                    Vector3.up),
                    1 - Mathf.Exp(-1F * fdt)
            );
        }
    }

    protected override void OnStateInitialize() {
    
    }

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