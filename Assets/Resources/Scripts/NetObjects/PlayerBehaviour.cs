using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wrappers;

public class PlayerBehaviour : NetObject
{
    public bool isOwner = false;
    float moveSpeed = 5f;


    // Last state variables
    bool actionsChanged = false;
    List<PlayerActions> pendingActions = new List<PlayerActions>();
    bool positionChanged = false;
    Vector3 lastPosition;
    //bool rotationChanged = false;
    //private Quaternion lastRotation;

    private void Update()
    {
        if(isOwner)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        List<PlayerActions.Actions> actionsInFrame = new List<PlayerActions.Actions>();

        if (Input.GetKey(KeyCode.W))
        {
            actionsInFrame.Add(PlayerActions.Actions.MoveF);
        }
        if (Input.GetKey(KeyCode.S))
        {
            actionsInFrame.Add(PlayerActions.Actions.MoveB);
        }
        if (Input.GetKey(KeyCode.A))
        {
            actionsInFrame.Add(PlayerActions.Actions.MoveL);
        }
        if (Input.GetKey(KeyCode.D))
        {
            actionsInFrame.Add(PlayerActions.Actions.MoveR);
        }

        if (actionsInFrame.Count > 0)
        {
            PlayerActions actionsToExecute = new PlayerActions(actionsInFrame, Time.deltaTime.ToString());

            ExecuteActions(actionsToExecute);

            pendingActions.Add(actionsToExecute);
        }
    }

    public void ExecuteActions(PlayerActions actions)
    {
        foreach (var action in actions.a)
        {
            switch (action)
            {
                case PlayerActions.Actions.MoveF:
                case PlayerActions.Actions.MoveB:
                case PlayerActions.Actions.MoveL:
                case PlayerActions.Actions.MoveR:
                    Move(action, float.Parse(actions.p));
                    break;
                case PlayerActions.Actions.Rotate:
                    break;
                case PlayerActions.Actions.None:
                default:
                    break;
            }
        }

        
    }

    void Move(PlayerActions.Actions action, float deltaTime)
    {
        Vector3 direction = Vector3.zero;

        switch (action)
        {
            case PlayerActions.Actions.MoveF:
                direction = Vector3.forward;
                break;
            case PlayerActions.Actions.MoveB:
                direction = Vector3.back;
                break;
            case PlayerActions.Actions.MoveL:
                direction = Vector3.left;
                break;
            case PlayerActions.Actions.MoveR:
                direction = Vector3.right;
                break;
            case PlayerActions.Actions.None:
            default:
                break;
        }

        if (direction.magnitude > 0)
        {
            transform.Translate(direction * moveSpeed * deltaTime, Space.World);

            if (transform.position != lastPosition) positionChanged = true;
            
        }
    }
    void Rotate(Vector3 increment)
    {

    }

    public PlayerActionList? GetActionsList()
    {
        if (pendingActions.Count <= 0) { return null; }

        PlayerActionList listToReturn = new PlayerActionList(netID, pendingActions);

        pendingActions.Clear();

        return listToReturn;
    }

    public Dictionary<string, object> GetChangedComponents()
    {
        Dictionary<string, object> changes = new Dictionary<string, object>();

        if (positionChanged)
        {
            if (transform.position.x != lastPosition.x)
                changes["position.x"] = transform.position.x;
            if (transform.position.y != lastPosition.y)
                changes["position.y"] = transform.position.y;
            if (transform.position.z != lastPosition.z)
                changes["position.z"] = transform.position.z;

            lastPosition = transform.position;

            positionChanged = false;
        }

        if (actionsChanged)
        {
            var actionList = GetActionsList();
            if (actionList != null)
                changes["actions"] = actionList;

            actionsChanged = false;
        }

        return changes;
    }


    //In the case of the player, this serves as state confirmation
    public override void UpdateObject(object info)
    {
        Wrappers.Player pw = (Wrappers.Player)info;

        transform.position = pw.p;
        transform.rotation = pw.r;
    }
}
