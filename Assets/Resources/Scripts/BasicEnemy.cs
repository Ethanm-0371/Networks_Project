using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : NetObject
{
    public enum State
    {
        None,
        Idle,
        Chase
    }

    public State currentState = State.Idle;

    List<GameObject> playerList = new List<GameObject>();
    GameObject targetPlayer;

    [SerializeField] float loseRadius = 15.0f;
    [SerializeField] float detectionRadius = 10.0f;
    [SerializeField] float movementSpeed = 0.25f;

    public static int maxHealth = 100;
    public int currentHealth;

    private void Start()
    {
        GameObject.FindGameObjectsWithTag("Player", playerList);
        currentHealth = maxHealth;
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

    public override NetInfo GetNetInfo()
    {
        return new Wrappers.BasicZombie(this);
    }

    public override void UpdateObject(NetInfo info)
    {
        Wrappers.BasicZombie bzw = (Wrappers.BasicZombie)info;

        currentState = bzw.currentState;
        currentHealth = bzw.currentHealth;
    }

    public void InitZombie(bool isRoomZombie, int spawnPoint)
    {
        if (isRoomZombie)
        {
            currentState = State.Chase;
            loseRadius = 999f;
            movementSpeed *= 1.5f;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            GameServer.Singleton?.MarkObjectToDelete(netID);
        }
    }
}
