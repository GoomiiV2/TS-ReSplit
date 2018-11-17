using Assets.Scripts.TSFramework;
using System.Collections;
using System.Collections.Generic;
using TS2;
using UnityEngine;

public class DebugEntity : MonoBehaviour
{
    public bool DrawDebug     = true;
    public object Data        = null;

    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
        if (DrawDebug)
        {
            Gizmos.color = new Color32(224, 51, 94, 255);
            Gizmos.DrawSphere(transform.position, 0.2f);

            Gizmos.color = new Color32(239, 107, 48, 255);
            var endPos   = transform.position + (transform.forward * 1.0f);
            Gizmos.DrawRay(new Ray(transform.position, transform.forward));
        }
    }

    public static GameObject Create(Vector3 Pos, Vector3 Rot, string Name, object Data)
    {
        var obj        = new GameObject($"Debug Entity: {Name}");
        var dbgEntity  = obj.AddComponent<DebugEntity>();
        var newRot     = Utils.RadRotationToDeg(Rot);
        dbgEntity.Data = Data;

        obj.transform.SetPositionAndRotation(Pos, Quaternion.Euler(newRot));

        return obj;
    }
}
