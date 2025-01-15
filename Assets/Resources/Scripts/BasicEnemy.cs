using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy : NetObject
{
    public enum State
    {
        None,
        Idle,
        Chase
    }

    public State currentState = State.Idle;
    NavMeshAgent agent;

    List<GameObject> playerList = new List<GameObject>();
    GameObject targetPlayer;

    [SerializeField] float loseRadius = 15.0f;
    [SerializeField] float detectionRadius = 10.0f;
    [SerializeField] float movementSpeed = 0.25f;

    public static int maxHealth = 100;
    public int currentHealth;
    bool isDead = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        currentHealth = maxHealth;
    }

    private void Update()
    {
        CheckState();
        ExecuteState();
    }

    void CheckState()
    {
        GameObject.FindGameObjectsWithTag("Player", playerList);

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
            agent.SetDestination(transform.position);

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
        transform.Rotate(Vector3.up * 2.0f);
    }
    void DoChase()
    {
        agent.SetDestination(targetPlayer.transform.position);
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
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            GameServer.Singleton?.MarkObjectToDelete(netID);
        }
        else if(currentHealth > 0)
        {
            currentHealth -= amount;
        }
    }
}
