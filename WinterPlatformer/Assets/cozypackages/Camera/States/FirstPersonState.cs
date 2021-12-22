using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonState : CameraState
{
    private float ease;

    protected override void OnStateInitialize()
    {
    
    }

    public override void Enter(CameraState prev)
    {
    
    }

    public override void Exit(CameraState next)
    {
        ease = 0F;
        Machine.ApplyOrbitPosition();
    }

    public override void FixedTick(float fdt)
    {
        const float max = 0.25F;

        PlayerInput PlayerInput = Machine.GetPlayerInput;
        Vector2 Mouse = PlayerInput.GetRawMouse;
        
        // reverse in FP mode
        Mouse[1] *= -1F;
        
        Machine.ComputeEasingTime(
            Mouse.sqrMagnitude,
            ref ease,
            max,
            fdt
        );

        float amt = Machine.ComputeEasingAmount(ease / max, fdt);
        Machine.OrbitAroundTarget(Mouse * amt);
        Machine.ApplyOrbitPosition();

        Machine.ApplyOrbitPosition(0F);
    }

    public override void Tick(float dt)
    {
    
    }
}
