using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualOrbitState : CameraState
{
    private float ease;

    protected override void OnStateInitialize() { }

    public override void Enter(CameraState prev) { }

    public override void Exit(CameraState next)
    {
        ease = 0F;
        Machine.ApplyOrbitPosition();
    }

    public override void FixedTick(float fdt)
    {
        const float max = 0.5F;

        PlayerInput PlayerInput = Machine.GetPlayerInput;
        Vector2 Mouse = PlayerInput.GetRawMouse;
        
        if(AwaitTransitionToAlign(PlayerInput.GetLeftTrigger))
            return;

        Machine.ComputeEasingTime(
            Mouse.sqrMagnitude,
            ref ease,
            max,
            fdt
        );

        float amt = Machine.ComputeEasingAmount(
            ease / max,
            fdt
        );

        Machine.OrbitAroundTarget(Mouse * amt);
        Machine.ApplyOrbitPosition();
    }

    public override void Tick(float dt) { }

    bool AwaitTransitionToAlign(bool left_trigger) 
    {
        if (left_trigger)
        {
            Machine.GetFSM.SwitchState(
            (CameraState next) => 
            {
                ((AlignOrbitState) next).Prepare();
            }, "Align");
            return true;
        }
        else
            return false;
    }
}