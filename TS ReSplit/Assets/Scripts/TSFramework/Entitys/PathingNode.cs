using Assets.Scripts.TSFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingNode : MonoBehaviour
{
    public uint ID;
    public List<GameObject> LinkedNodes;
    public TS2.Pathing.Node NodeData;
    private bool IsExtraNode = false;
    public bool DrawDebug    = true;

    public void SetNodeData(TS2.Pathing.Node Node, bool IsExtraNode = false)
    {
        NodeData                = Node;
        ID                      = Node.ID;
        transform.position      = Utils.V3FromFloats(Node.Position);
        this.IsExtraNode        = IsExtraNode;
    }

    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
        if (DrawDebug)
        {
            Gizmos.color = IsExtraNode ? Color.magenta : Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.2f);

            // Draw the links
            for (int i = 0; i < LinkedNodes.Count; i++)
            {
                var linkedGO = LinkedNodes[i];
                Gizmos.color = IsExtraNode ? Color.green : Color.yellow;
                Gizmos.DrawLine(transform.position, linkedGO.transform.position);
            }
        }
    }
}
