using Assets.Scripts.TSFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisLeaf : MonoBehaviour
{
    public bool DrawDebug = true;
    public PlanePoints Points;

    public TS2.VisPortal RawVisLeafData;

    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
        if (DrawDebug)
        {
            Gizmos.color = new Color32(224, 51, 94, 255);
            Gizmos.DrawSphere(transform.position, 0.2f);

            Gizmos.DrawCube(transform.position, Points.Bounds.size);
            Gizmos.matrix = transform.localToWorldMatrix;
        }
    }

    public static GameObject Create(TS2.VisPortal VisPortal)
    {
        var gObj       = new GameObject();
        var visLeaf    = gObj.AddComponent<VisLeaf>();

        visLeaf.RawVisLeafData = VisPortal;
        visLeaf.Points         = new PlanePoints()
        {
            TopLeft     = Utils.V3FromFloats(VisPortal.Points[0]),
            TopRight    = Utils.V3FromFloats(VisPortal.Points[1]),
            BottomLeft  = Utils.V3FromFloats(VisPortal.Points[2]),
            BottomRight = Utils.V3FromFloats(VisPortal.Points[3])
        };

        visLeaf.Points.Bounds = new Bounds();
        for (int i = 0; i < 4; i++)
        {
            visLeaf.Points.Bounds.Encapsulate(Utils.V3FromFloats(VisPortal.Points[i]));
        }

        gObj.transform.position = visLeaf.Points.Bounds.center;
        

        return gObj;
    }

    public struct PlanePoints
    {
        public Vector3 TopLeft;
        public Vector3 TopRight;
        public Vector3 BottomLeft;
        public Vector3 BottomRight;
        public Bounds Bounds;

        public Vector3 GetDims()
        {
            var dims = new Vector3()
            {
                x = (TopRight.x - BottomLeft.x) /2,
                y = 0.01f,
                z = (TopRight.z - BottomLeft.z) /2
            };

            return dims;
        }
    }
}
