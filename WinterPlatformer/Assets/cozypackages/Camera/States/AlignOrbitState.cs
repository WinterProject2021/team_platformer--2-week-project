using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignOrbitState : CameraState
{
    [SerializeField] private AnimationCurve AlignCurve;

    private Quaternion init, final;
    private float ease;
    
    protected override void OnStateInitialize() { }

    public void Prepare() => Machine.ComputeRealignments(ref init, ref final);

    public override void Enter(CameraState prev) { }

    public override void Exit(CameraState next)
    {
        ease = 0F;
        Machine.ApplyOrbitPosition();
    }

    public override void FixedTick(float fdt)
    {
        const float max = 0.5F;

        if(ease >= max)
        {
            Machine.GetFSM.SwitchState("Automatic");
            return;
        }
        else 
        {
            ease += fdt;
            ease = Mathf.Min(ease, max);

            float rate = AlignCurve.Evaluate(ease / max);

            Machine.SetViewRotation(
                Quaternion.Slerp(
                    init,
                    final,
                    rate)
            );
            Machine.ApplyOrbitPosition();
        }
    }

    public override void Tick(float dt) { }
}
