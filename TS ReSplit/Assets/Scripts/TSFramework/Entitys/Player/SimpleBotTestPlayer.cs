using Assets.Scripts.TSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

// A simple test bot to walk about
[RequireComponent(typeof(NavMeshAgent))]
public class SimpleBotTestPlayer : MonoBehaviour
{
    private NavMeshAgent NavAgent;
    private Bounds NavMeshBounds;
    private bool HasReachedGoal = false;

    public float GoalReachedThreshold = 0.2f;
    public MeshFilter BoundsMesh;
    public bool FollowPlayer = false;
    private GameObject PlayerGO = null;

    public void Start()
    {
        NavAgent          = GetComponent<NavMeshAgent>();
        //NavMeshBounds   = Utils.CalcNavMeshBounds();
        //NavMeshBounds     = BoundsMesh.mesh.bounds;

        if (FollowPlayer)
        {
            PlayerGO = GameObject.Find("Player");
        }
        else
        {
            PickRandWalkGoal();
        }
    }

    void Update()
    {
        if (FollowPlayer && PlayerGO != null)
        {
            NavAgent.SetDestination(PlayerGO.transform.position);
        }
        else
        {
            var goalDist = Vector3.Distance(transform.position, NavAgent.destination);
            if (!HasReachedGoal && goalDist <= GoalReachedThreshold)
            {
                HasReachedGoal = true;
                NavAgent.isStopped = true;
                Invoke("PickRandWalkGoal", 5); // wait abit before moving again
            }
        }
    }

    public void PickRandWalkGoal()
    {
        //var randPoint = PickRandomPointInMap();
        var randPoint = GetRandomLocation();

        /*if (Vector3.Distance(randPoint, NavAgent.destination) <= 10)
        {
            var dir = randPoint - NavMeshBounds.center;
            dir.Normalize();
            randPoint = randPoint - (dir * 20);
        }*/

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

    Vector3 GetRandomLocation()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        int t = Random.Range(0, navMeshData.indices.Length - 3);

        Vector3 point = Vector3.Lerp(navMeshData.vertices[navMeshData.indices[t]], navMeshData.vertices[navMeshData.indices[t + 1]], Random.value);
        Vector3.Lerp(point, navMeshData.vertices[navMeshData.indices[t + 2]], Random.value);

        return point;
    }

    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
        #if UNITY_EDITOR
            if (NavAgent != null)
            {
                Gizmos.color = new Color32(224, 51, 94, 255);
                Handles.Label(NavAgent.destination, "Bot Goal");
                Gizmos.DrawSphere(NavAgent.destination, 0.2f);
            }
        #endif
    }
}