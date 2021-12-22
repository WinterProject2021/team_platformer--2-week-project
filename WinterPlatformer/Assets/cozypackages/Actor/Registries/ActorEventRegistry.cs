using System;
using UnityEngine;

public class ActorEventRegistry : MonoBehaviour
{
    public Action Event_ActorJumped;
    public Action Event_ActorLanded;

    public Action<bool> Event_ActorToggledView;

    public Action<Vector3> Event_ActorFoundLedge;
    public Action<Quaternion> Event_ActorTurn;

    public Action<Vector3> Func_SetCameraPosition;
    public Action<Quaternion> Func_SetCameraOrientation;

    public Func<Vector3> Func_GetCameraPosition;
    public Func<Quaternion> Func_GetCameraOrientation;

#pragma warning disable IDE0051 // Remove unused private members
    void Start()
#pragma warning restore IDE0051 // Remove unused private members
    {
        Event_ActorJumped      = null;
        Event_ActorLanded      = null;
        Event_ActorFoundLedge  = null;
        Event_ActorToggledView = null;
    
        Func_SetCameraPosition = null;
        Func_SetCameraOrientation = null;

        Func_GetCameraPosition = null;
        Func_GetCameraOrientation = null;
    }
}
