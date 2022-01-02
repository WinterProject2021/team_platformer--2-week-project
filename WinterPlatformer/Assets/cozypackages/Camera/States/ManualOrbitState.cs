using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualOrbitState : CameraState
{
    private float ease;

    protected override void OnStateInitialize() { Machine.GetFSM.SetState(this.GetKey); }

    public override void Enter(CameraState prev) { }

    public override void Exit(CameraState next)
    {
        ease = 0F;
        Machine.ApplyOrbitPosition();
    }

    public override void FixedTick(float fdt) {
        
    }

    public override void Tick(float dt) { 
        Cursor.lockState = CursorLockMode.Locked;
        const float max = 0.5F;

        PlayerInput PlayerInput = Machine.GetPlayerInput;
        Vector2 Mouse = PlayerInput.GetRawMouse;
        Mouse[1] *= -1F;
        
        // if(AwaitTransitionToAlign(PlayerInput.GetLeftTrigger))
        //     return;

        // Machine.ComputeEasingTime(
        //     Mouse.sqrMagnitude,
        //     ref ease,
        //     max,
        //     fdt
        // );

        // float amt = Machine.ComputeEasingAmount(
        //     ease / max,
        //     fdt
        // );

        float amt = 1;

        Machine.OrbitAroundTarget(Mouse * amt);

        // Vector3 p = Machine.ViewPosition;

        Machine.ApplyOrbitPosition();

        // Machine.SetViewPosition(
        //     Vector3.Lerp(
        //         p,
        //         Machine.ViewPosition,
        //         1F - Mathf.Exp(-360F * dt))
        // );
    }

    // bool AwaitTransitionToAlign(bool left_trigger) 
    // {
    //     if (left_trigger)
    //     {
    //         Machine.GetFSM.SwitchState(
    //         (CameraState next) => 
    //         {
    //             // ((AlignOrbitState) next).Prepare();
    //         }, "Align");
    //         return true;
    //     }
    //     else
    //         return false;
    // }
}