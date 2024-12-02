using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    enum State
    {
        None,
        Idle,
        Chase
    }

    State currentState = State.Idle;

    List<GameObject> playerList = new List<GameObject>();
    GameObject targetPlayer;

    [SerializeField] float detectionRadius = 10.0f;
    [SerializeField] float loseRadius = 15.0f;
    [SerializeField] float movementSpeed = 0.25f;

    private void Start()
    {
        GameObject.FindGameObjectsWithTag("Player", playerList);
    }

    private void Update()
    {
        CheckState();
        ExecuteState();
    }

    void CheckState()
    {
        float smallestDistance = Vector3.Distance(transform.position, playerList[0].transform.position);
        targetPlayer = playerList[0];

        foreach (var item in playerList)
        {
            float newDistance = Vector3.Distance(transform.position, item.transform.position);
            if (newDistance < smallestDistance)
            {
                smallestDistance = newDistance;
                targetPlayer = item;
            }
        }

        if (currentState != State.Chase && smallestDistance < detectionRadius)
        {
            currentState = State.Chase;
        }
        else if (currentState != State.Idle && smallestDistance > loseRadius)
        {
            currentState = State.Idle;
        }
    }

    void ExecuteState()
    {
        switch (currentState)
        {
            case State.Idle:
                DoIdle();
                break;
            case State.Chase:
                DoChase();
                break;
            case State.None:
            default:
                Debug.LogError("Error");
                break;
        }
    }

    void DoIdle()
    {
        transform.Rotate(Vector3.up * 10.0f);
    }
    void DoChase()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPlayer.transform.position, movementSpeed * Time.deltaTime);
        transform.LookAt(targetPlayer.transform);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.DrawWireSphere(transform.position, loseRadius);
    }
}
