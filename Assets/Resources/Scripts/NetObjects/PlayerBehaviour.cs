using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wrappers;

public class PlayerBehaviour : NetObject
{
    public bool isOwner = false;
    float moveSpeed = 5f;

    List<List<PlayerAction>> pendingActions = new List<List<PlayerAction>>();

    private void Update()
    {
        if(isOwner)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        List<PlayerAction> actionList = new List<PlayerAction>();

        if (Input.GetKey(KeyCode.W))
        {
            actionList.Add(new PlayerAction(PlayerAction.Actions.Move, new PlayerActionParams(Vector3.forward * Time.deltaTime)));
        }
        if (Input.GetKey(KeyCode.S))
        {
            actionList.Add(new PlayerAction(PlayerAction.Actions.Move, new PlayerActionParams(Vector3.back * Time.deltaTime)));
        }
        if (Input.GetKey(KeyCode.A))
        {
            actionList.Add(new PlayerAction(PlayerAction.Actions.Move, new PlayerActionParams(Vector3.left * Time.deltaTime)));
        }
        if (Input.GetKey(KeyCode.D))
        {
            actionList.Add(new PlayerAction(PlayerAction.Actions.Move, new PlayerActionParams(Vector3.right * Time.deltaTime)));
        }

        foreach (var action in actionList)
        {
            ExecuteAction(action);
        }

        if (actionList.Count > 0) { pendingActions.Add(actionList); }
    }

    public void ExecuteAction(PlayerAction action)
    {
        switch (action.a)
        {
            case PlayerAction.Actions.Move:
                Move(JsonUtility.FromJson<Vector3>(action.p.l[0]));
                break;
            case PlayerAction.Actions.Rotate:
                //Rotate((Vector3)action.p[0]); //To be implemented
                break;
            case PlayerAction.Actions.None:
            default:
                break;
        }
    }

    void Move(Vector3 direction)
    {
        if (direction.magnitude > 0)
        {
            transform.Translate(direction * moveSpeed, Space.World);
        }
    }
    void Rotate(Vector3 increment)
    {

    }

    public List<List<PlayerAction>> GetActionsList()
    {
        if (pendingActions.Count <= 0) { return null; }

        List<List<PlayerAction>> listToReturn = new List<List<PlayerAction>>(pendingActions);

        pendingActions.Clear();

        return listToReturn;
    }

    //In the case of the player, this serves as state confirmation
    public override void UpdateObject(object info)
    {
        Wrappers.Player pw = (Wrappers.Player)info;

        transform.position = pw.p;
        transform.rotation = pw.r;
    }
}
