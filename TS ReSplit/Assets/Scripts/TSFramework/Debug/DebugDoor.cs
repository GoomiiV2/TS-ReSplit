using Assets.Scripts.TSFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS2;
using UnityEngine;

public class DebugDoor : MonoBehaviour
{
    public bool DrawDebug = true;
    public PortalDoor DoorData;

    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
        if (DrawDebug)
        {
            Gizmos.color = new Color32(224, 51, 94, 255);
            Gizmos.DrawSphere(transform.position, 0.2f);

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Utils.V3FromFloats(DoorData.Dimensions));
        }
    }

    public static GameObject Create(Vector3 Pos, Vector3 Dims, string Name, PortalDoor DoorData)
    {
        var obj                     = new GameObject($"Debug Door: {Name}");
        var dbgEntity               = obj.AddComponent<DebugDoor>();
        obj.transform.position      = Pos;
        obj.transform.rotation      = Quaternion.Euler(0, Mathf.Rad2Deg * DoorData.Angle, 0);
        dbgEntity.DoorData          = DoorData;

        return obj;
    }
}
