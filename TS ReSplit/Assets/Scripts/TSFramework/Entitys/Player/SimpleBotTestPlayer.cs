using Assets.Scripts.TSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// A simple test bot to walk about
[RequireComponent(typeof(NavMeshAgent))]
public class SimpleBotTestPlayer : MonoBehaviour
{
    private NavMeshAgent NavAgent;
    private Bounds NavMeshBounds;
    private bool HasReachedGoal = false;

    public float GoalReachedThreshold = 0.2f;
    public MeshFilter BoundsMesh;

    public void Start()
    {
        NavAgent          = GetComponent<NavMeshAgent>();
        //NavMeshBounds   = Utils.CalcNavMeshBounds();
        NavMeshBounds     = BoundsMesh.mesh.bounds;
        PickRandWalkGoal();
    }

    void Update()
    {
        var goalDist = Vector3.Distance(transform.position, NavAgent.destination);
        if (!HasReachedGoal && goalDist <= GoalReachedThreshold)
        {
            HasReachedGoal     = true;
            NavAgent.isStopped = true;
            Invoke("PickRandWalkGoal", 5); // wait abit before moving again
        }
    }

    public void PickRandWalkGoal()
    {
        var randPoint = PickRandomPointInMap();

        if (Vector3.Distance(randPoint, NavAgent.destination) <= 10)
        {
            var dir = randPoint - NavMeshBounds.center;
            dir.Normalize();
            randPoint = randPoint - (dir * 20);
        }

        NavAgent.SetDestination(randPoint);
        HasReachedGoal     = false;
        NavAgent.isStopped = false;
    }

    public Vector3 PickRandomPointInMap(float MaxDist = 200f)
    {
        var randPoint = Utils.RandPointInBounds(NavMeshBounds);
        NavMesh.SamplePosition(randPoint, out NavMeshHit navHit, MaxDist, NavMesh.AllAreas);
        return navHit.position;
    }

    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
        if (NavAgent != null)
        {
            Gizmos.color = new Color32(224, 51, 94, 255);
            Handles.Label(NavAgent.destination, "Bot Goal");
            Gizmos.DrawSphere(NavAgent.destination, 0.2f);
        }
    }
}