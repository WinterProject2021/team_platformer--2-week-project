using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using com.cozyhome.Timers;
using com.cozyhome.Actors;
using com.cozyhome.Console;
using com.cozyhome.Archetype;
using com.cozyhome.ChainedExecutions;
using com.cozyhome.Vectors;

[System.Serializable]
public class HookshotMiddleman 
{
    [SerializeField] private LineRenderer hookshot_render;
    [SerializeField] private LayerMask    hookshot_mask;
    [SerializeField] private GameObject   crosshair;

    private RaycastHit[] internalhits;
    private Ray[]        internalrays;
    private PlayerInput  input;

    private float raylength, time;
    private int   hitindex;

    // camera calls
    private Action<Vector3> assign_camerapos;
    private Action<Quaternion> assign_cameraorientation;

    private Func<Vector3> get_camerapos;
    private Func<Quaternion> get_cameraorientation;

    // actor calls
    private Action<Vector3>                   assign_actorvel;
    private Action<Vector3>                   assign_actorpos;
    private Action<bool>                    assign_enablesnap;
    private Action<Quaternion>        assign_actororientation;
    private Action<ActorHeader.MoveType> assign_actormovetype;

    private Func<Vector3>                   get_actorvel;
    private Func<Vector3>                   get_actorpos;
    private Func<ArchetypeHeader.Archetype> get_archetype;
    private Func<Quaternion>                get_actororientation;

    public void AllocHits() 
    { 
        raylength = 15F;
        internalhits = new RaycastHit[10]; 
        internalrays = new Ray[10];
    }

    public void AssignValues(PlayerInput input, 
        float time)
    {
        this.input        = input;
        this.time         = time;
    }

    public void AssignCameraSets(
        Action<Vector3> assign_camerapos,
        Action<Quaternion> assign_cameraorientation) 
    {
        this.assign_camerapos = assign_camerapos;
        this.assign_cameraorientation = assign_cameraorientation;
    }

    public void AssignCameraGets(
        Func<Vector3> get_camerapos, 
        Func<Quaternion> get_cameraorientation)
    {
        this.get_camerapos = get_camerapos;
        this.get_cameraorientation = get_cameraorientation;
    }

    public void AssignActorSets(
        Action<Vector3>                       assign_actorvel,
        Action<Vector3>                       assign_actorpos, 
        Action<bool>                        assign_enablesnap,
        Action<Quaternion>            assign_actororientation,
        Action<ActorHeader.MoveType>     assign_actormovetype)
    {
        this.assign_actorvel = assign_actorvel;
        this.assign_actorpos = assign_actorpos;
        this.assign_enablesnap = assign_enablesnap;
        this.assign_actororientation = assign_actororientation;
        this.assign_actormovetype = assign_actormovetype;
    }
    
    public void AssignGets(
        Func<Vector3>                   get_actorvel,
        Func<Vector3>                   get_actorpos, 
        Func<Quaternion>                get_actororientation,
        Func<ArchetypeHeader.Archetype> get_archetype)
    {
        this.get_actorvel         = get_actorvel;
        this.get_actorpos         = get_actorpos;
        this.get_actororientation = get_actororientation;
        this.get_archetype        = get_archetype; 
    }

    public void WriteRayAtHitIndex(Ray ray) => internalrays[hitindex] = ray;
    public void WriteHitIndex(int i0) => hitindex = i0;

    public void WriteLinePositions(int i, Vector3 a0) => hookshot_render.SetPosition(i, a0);

    public float      RayLength    => raylength;
    public float      Time         => time;

    public bool       HitSomething => hitindex >= 0;
    public bool       SquareTrigger => input.GetSquareTrigger;

    public GameObject   GetCrosshair   => crosshair;
    
    public RaycastHit[] InternalHits => internalhits;
    public RaycastHit   RegisteredHit => internalhits[hitindex];
    public Ray          RegisteredRay => internalrays[hitindex];

    public LayerMask Mask => hookshot_mask;

    public void SetCameraPosition(Vector3 pos) => assign_camerapos(pos);
    public void SetCameraOrientation(Quaternion orient) => assign_cameraorientation(orient);

    public Vector3 GetCameraPosition() => get_camerapos();
    public Quaternion GetCameraOrientation() => get_cameraorientation();

    public void SetActorVelocity(Vector3 vel) => assign_actorvel(vel);
    public void SetActorPosition(Vector3 pos) => assign_actorpos(pos);
    public void SetActorSnapEnabled(bool enable) => assign_enablesnap(enable);
    public void SetActorOrientation(Quaternion orient) => assign_actororientation(orient);
    public void SetActorMoveType(ActorHeader.MoveType moveType) => assign_actormovetype(moveType);

    public Vector3 GetActorVelocity()               => get_actorvel();
    public Vector3 GetActorPosition()               => get_actorpos();
    public Quaternion GetActorOrientation()         => get_actororientation();
    public ArchetypeHeader.Archetype GetArchetype() => get_archetype();

}

// Fairly straight-forward single dimensional mapping between two points in three dimensional space
// may need to be refactored as if obstructions subtract/slide velocity along surface normals, distance
// may be lost. 

// A straight-forward derivative approach is not smart, maybe have a distance based heuristic instead of
// time based.
[System.Serializable]
public class TravelExecution : ExecutionChain<int, HookshotMiddleman>.Execution
{   
    public Action OnFinish;
    [SerializeField] private float speed = 10;
    [SerializeField] private TimerHeader.SnapshotTimer Timer;
    // [SerializeField] private AnimationCurve TravelCurve;

    private float Distance;
    private Vector3 start, final;

    public override void Enter(HookshotMiddleman Middleman)
    {
        Vector3 max = Middleman.GetArchetype().MaximizeConvexBoundary(
            (Middleman.GetActorOrientation(), Middleman.RegisteredHit.point, Vector3.zero),
            (-Middleman.RegisteredHit.normal, Middleman.RegisteredHit.point)
        );

        float tt = VectorHeader.LinePlaneIntersection(
            (max, -Middleman.RegisteredRay.direction), 
            (Middleman.RegisteredHit.point, -Middleman.RegisteredHit.normal)
        );

        Debug.Log(tt);

        Distance = Middleman.RegisteredHit.distance - tt;
        Timer.Stamp(Middleman.Time);
    
        start = Middleman.GetCameraPosition();
        final = Middleman.RegisteredRay.origin + Middleman.RegisteredRay.direction * Distance;
    
        // this is where we'll prepare the viewing position for the camera

        Middleman.SetActorSnapEnabled(false);
    }

    public override void Exit(HookshotMiddleman Middleman)
    {
        Middleman.SetActorSnapEnabled(true);
        Middleman.SetActorVelocity(Vector3.zero);
        Middleman.SetActorMoveType(ActorHeader.MoveType.SlideStep);
    }

    public override ExecutionState Execute(HookshotMiddleman Middleman)
    {
        // if(Timer.Max * Timer.Difference(Time.time) > Distance)
        // {
        //     OnFinish?.Invoke();
        //     return ExecutionState.FINISHED;
        // }
        // else 
        // {
        // float d = Timer.Max * Timer.Difference(Time.time) / (Distance);
        // float t = TravelCurve.Evaluate(d);
    
        // Vector3 p = (1 - t) * start + t * final;
        
        // point where we are at currently 
        Vector3 p    = Middleman.GetActorPosition();
        Vector3 di_1 = final - p;
        float d1_m = di_1.magnitude;

        Vector3 v = d1_m > 0 ? (di_1) * speed / d1_m : Vector3.zero;

        // not pointing in the same direction, last v is last frame v
        float angular_frame_delta = Vector3.Dot(Middleman.GetActorVelocity(), v);
        const float loss_amt = 0.1F;

        Debug.Log(d1_m);

        if(d1_m < loss_amt && angular_frame_delta < 0.01F)
        {
            // endearly
            return ExecutionState.FINISHED;
        }

        float t = (p - start).magnitude / (final - start).magnitude;

        Middleman.WriteLinePositions(0, a0: p - Vector3.up * 0.25F);
        
        Middleman.SetCameraPosition(start);
        Middleman.SetActorVelocity(v);

        if(t > 0)
            Middleman.SetCameraOrientation(Quaternion.LookRotation(p - start, Vector3.up));

        return ExecutionState.ACTIVE;    
    
        // }
    }
}

// simply the concurrent execution that draws the hookshot line in order to visually display what is happening

[System.Serializable]
public class ShootExecution : ExecutionChain<int, HookshotMiddleman>.Execution
{
    public Action OnFinish;

    [SerializeField] private TimerHeader.SnapshotTimer Timer;
    [SerializeField] private AnimationCurve LineCurve;

    private float Distance;

    public override void Enter(HookshotMiddleman Middleman)
    {
        this.Distance = Middleman.RegisteredHit.distance;

        Middleman.GetCrosshair.SetActive(false);
        Timer.Stamp(Middleman.Time);
    }

    public override void Exit(HookshotMiddleman Middleman)
    {
        Timer.Stamp(Middleman.Time);
    }

    public override ExecutionState Execute(HookshotMiddleman Middleman)
    {
        if(Timer.Max * Timer.Difference(Time.time) > Distance)
        {
            OnFinish?.Invoke();
            return ExecutionState.FINISHED;
        }
        else
        {
            Vector3 start = Middleman.GetCameraPosition() - Vector3.up * 0.25F;
            Vector3 end   = Middleman.RegisteredHit.point;

            float d = Timer.Max * Timer.Difference(Time.time) / (Distance);
            float t = LineCurve.Evaluate(d);
            
            Middleman.WriteLinePositions(0, a0: start);
            Middleman.WriteLinePositions(1, a0: (1 - t) * start + t * end);

            return ExecutionState.ACTIVE;
        }
    }
}

[System.Serializable]
public class AimExecution : ExecutionChain<int, HookshotMiddleman>.Execution
{
    public Action OnFinish;

    public override void Enter(HookshotMiddleman Middleman)
    {
    
    }

    public override void Exit(HookshotMiddleman Middleman)
    {
        // add shoot to execution chain
        OnFinish?.Invoke();

        Middleman.GetCrosshair.SetActive(false);
    }

    public override ExecutionState Execute(HookshotMiddleman Middleman)
    {
        int nbhits = ArchetypeHeader.TraceRay(
            Middleman.GetCameraPosition(),
            Middleman.GetCameraOrientation() * Vector3.forward,
            Middleman.RayLength,
            Middleman.InternalHits,
            Middleman.Mask
        );

        ArchetypeHeader.TraceFilters.FindClosestFilterInvalids(
            ref nbhits,
            out int i0,
            0F,
            null,
            Middleman.InternalHits
        );

        Middleman.WriteHitIndex(i0);
        
        if(i0 >= 0)
        {
            Middleman.WriteRayAtHitIndex(
                new Ray(
                    origin: Middleman.GetCameraPosition(),
                    direction: Middleman.GetCameraOrientation() * Vector3.forward
                )
            );

            // Rewrite distance
            // Compute resolution vector

            Middleman.GetCrosshair.SetActive(true);
            Middleman.GetCrosshair.transform.SetPositionAndRotation(
                Middleman.InternalHits[i0].point + Middleman.InternalHits[i0].normal * 0.01F,
                Quaternion.LookRotation(Middleman.InternalHits[i0].normal, Vector3.up)
            );

            if(Middleman.SquareTrigger)
                return ExecutionState.FINISHED;   
        }
        else
            Middleman.GetCrosshair.SetActive(false);

        return ExecutionState.ACTIVE;
    }

    void ComputeResolve(HookshotMiddleman man, ArchetypeHeader.Archetype arc, (Vector3 p, Vector3 n, Vector3 d) vec)
    {
        // find max neg dot
        // 8 vertices in box
        // 2 verts in capsule
        // 1 vert in sphere
        Vector3 p = arc.MaximizeConvexBoundary((
                man.GetActorOrientation(),
                man.GetActorPosition(),
                Vector3.zero
            ),
            (vec.n, vec.p)
        );

        float t = VectorHeader.LinePlaneIntersection((p, vec.d), (vec.p, vec.n));
        Debug.Log(t);
    }
}


// rework this into "Aim State"
// have hookshot be a literal hook class that will
// take this and do stuff!

// this way other items like the bow and slingshot, etc. can take
// advantage of the shoot functionality...
public class HookshotState : ActorState
{
    private ExecutionChain<int, HookshotMiddleman> EChain;
    [SerializeField] private HookshotMiddleman     Middleman;
    [SerializeField] private TravelExecution       Travel;
    [SerializeField] private ShootExecution        Shoot;
    [SerializeField] private AimExecution          Aim;

    protected override void OnStateInitialize() 
    {
        var registry       = Machine.GetActorEventRegistry;
        var actor          = Machine.GetActor;
        
        // MonoConsole.AttemptExecution("act_sw Hookshot ");
        Middleman.AllocHits();
        EChain = new ExecutionChain<int, HookshotMiddleman>(Middleman);

        Travel.OnFinish += () => {
            // EChain.AddExecution(Aim);
            Machine.GetFSM.SwitchState("Ground");
        };

        Shoot.OnFinish += () => {
            EChain.AddExecution(Travel);
        };

        Aim.OnFinish += () => {
            EChain.AddExecution(Shoot);
        };

        Middleman.AssignCameraSets(
            (Vector3 x) => registry.Func_SetCameraPosition(x),
            (Quaternion y) => registry.Func_SetCameraOrientation(y)
        );
        
        Middleman.AssignCameraGets(
            () => registry.Func_GetCameraPosition(),
            () => registry.Func_GetCameraOrientation()
        );

        Middleman.AssignActorSets(
            (Vector3 v)              => actor.SetVelocity(v),
            (Vector3 x)              => actor.SetPosition(x),
            (bool b)                 => actor.SetSnapEnabled(b),
            (Quaternion y)           => actor.SetOrientation(y),
            (ActorHeader.MoveType m) => actor.SetMoveType(m)
        );

        Middleman.AssignGets(
            () => actor.velocity,
            () => actor.position,
            () => actor.orientation,
            () => actor.GetArchetype()
        );

    }

    public override void Enter(ActorState prev)
    {
        EChain.AddExecution(Aim);
        // this.Machine.GetActorEventRegistry.Event_ActorToggledView(true);
    }

    public override void Exit(ActorState next)
    {
        this.Machine.GetActorEventRegistry.Event_ActorToggledView(false);
    }

    public override void Tick(float fdt)
    {
        var time           = Time.time;
        var input          = Machine.GetPlayerInput;

        Middleman.AssignValues(input, time);

        EChain.Tick();
    }

    public override void OnGroundHit(ActorHeader.GroundHit ground, ActorHeader.GroundHit lastground, LayerMask layermask) {}
    public override void OnTraceHit(ActorHeader.TraceHitType tracetype, RaycastHit trace, Vector3 position, Vector3 velocity) {}
    public override void OnTriggerHit(ActorHeader.TriggerHitType triggertype, Collider trigger) {}

}
