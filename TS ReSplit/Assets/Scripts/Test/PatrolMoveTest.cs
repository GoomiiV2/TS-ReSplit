using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PatrolMoveTest : MonoBehaviour {
    public GameObject[] PatrolPoints   = new GameObject[4];
    public float HasReachedThreshold   = 1f;
    public float StartTurningThreshold = 3.0f;
    public float MoveSpeed             = 0.00001f;

    private int LastGoal = 0;
    private bool IsTurning = false;
    private Quaternion RotationGoal;

    void Awake()
    {
        transform.position = PatrolPoints[0].transform.position;
        transform.LookAt(PatrolPoints[1].transform);

        for (int i = 0; i < PatrolPoints.Length; i++)
        {
            if (i == PatrolPoints.Length - 1)
            {
                PatrolPoints[i].transform.LookAt(PatrolPoints[0].transform);
            }
            else
            {
                PatrolPoints[i].transform.LookAt(PatrolPoints[i+1].transform);
            }
        }

        transform.LookAt(GetNextGoal());
    }

    void Update()
    {
        var     goalDist  = Vector3.Distance(transform.position, GetNextGoal().position);
        Vector3 direction = GetNextGoal().position - transform.position;

        if (goalDist <= StartTurningThreshold)
        {
            IsTurning = true;
            RotationGoal = GetNextGoal().rotation;
        }

        if (IsTurning)
        {
            IsTurning = true;
            transform.rotation = Quaternion.Lerp(transform.rotation, RotationGoal, 2.5f * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, RotationGoal) <= 1.0f)
            {
                IsTurning = false;
            }
        }

        if (goalDist <= HasReachedThreshold)
        {
            LastGoal++;
            if (LastGoal >= PatrolPoints.Length)
            {
                LastGoal = 0;
                //transform.rotation = GetLastGoal().rotation;
            }
        }

        GetComponent<CharacterController>().Move(direction.normalized * MoveSpeed);
        //GetComponent<CharacterController>().Move(transform.forward * MoveSpeed);
    }

    private Transform GetLastGoal()
    {
        return PatrolPoints[LastGoal].transform;
    }

    private Transform GetNextGoal()
    {
        var goal = LastGoal + 1;
        if (goal >= PatrolPoints.Length)
        {
            goal = 0;
        }

        return PatrolPoints[goal].transform;
    }

    private Transform GetNextNextGoal()
    {
        var goal = LastGoal + 1;
        if (goal >= PatrolPoints.Length)
        {
            goal = 0;
        }

        var goal2 = goal + 1;
        if (goal2 >= PatrolPoints.Length)
        {
            goal2 = 0;
        }

        return PatrolPoints[goal2].transform;
    }

}