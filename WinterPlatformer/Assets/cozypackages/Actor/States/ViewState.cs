using System.Collections;
using System.Collections.Generic;
using com.cozyhome.Actors;
using com.cozyhome.Timers;
using UnityEngine;

public class ViewState : ActorState
{
    [SerializeField] private TimerHeader.SnapshotTimer ExitDelay;

    protected override void OnStateInitialize()
    {
        
    }

    public override void Enter(ActorState prev)
    {
        ActorHeader.Actor actor = Machine.GetActor;   
        actor.velocity = Vector3.zero;
    
        ActorEventRegistry registry = Machine.GetActorEventRegistry;
        registry.Event_ActorToggledView(true);
    
        ExitDelay.Stamp(Time.time);

    }

    public override void Exit(ActorState next)
    {
        // ActorEventRegistry registry = Machine.GetActorEventRegistry;
        // registry.Event_ActorToggledView(false); // exit out
    }

    public override void Tick(float fdt)
    {
        PlayerInput input = Machine.GetPlayerInput;

        if(ExitDelay.Check(Time.time) && input.GetXTrigger)
            Machine.GetFSM.SwitchState("Hookshot");
    }

    public override void OnGroundHit(ActorHeader.GroundHit ground, ActorHeader.GroundHit lastground, LayerMask layermask)
    {
    
    }

    public override void OnTraceHit(ActorHeader.TraceHitType tracetype, RaycastHit trace, Vector3 position, Vector3 velocity)
    {
    
    }
    
    public override void OnTriggerHit(ActorHeader.TriggerHitType triggertype, Collider trigger)
    {
    
    }
}
