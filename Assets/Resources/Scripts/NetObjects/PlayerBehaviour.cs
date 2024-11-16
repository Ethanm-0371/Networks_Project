using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wrappers;

public class PlayerBehaviour : NetObject
{
    public bool isOwner = false;
    float moveSpeed = 5f;

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
            actionList.Add(new PlayerAction(PlayerAction.Actions.Move, Vector3.forward));
        }
        if (Input.GetKey(KeyCode.S))
        {
            actionList.Add(new PlayerAction(PlayerAction.Actions.Move, Vector3.back));
        }
        if (Input.GetKey(KeyCode.A))
        {
            actionList.Add(new PlayerAction(PlayerAction.Actions.Move, Vector3.left));
        }
        if (Input.GetKey(KeyCode.D))
        {
            actionList.Add(new PlayerAction(PlayerAction.Actions.Move, Vector3.right));
        }

        foreach (var action in actionList)
        {
            ExecuteAction(action);
        }

        //Send actions
    }

    public void ExecuteAction(PlayerAction action)
    {
        switch (action.a)
        {
            case PlayerAction.Actions.Move:
                Move((Vector3)action.p[0]);
                break;
            case PlayerAction.Actions.Rotate:
                Rotate((Vector3)action.p[0]);
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
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }
    void Rotate(Vector3 increment)
    {

    }

    //In the case of the player, this serves as state confirmation
    public override void UpdateObject(object info)
    {
        Wrappers.Player pw = (Wrappers.Player)info;

        transform.position = pw.p;
        transform.rotation = pw.r;
    }
}
