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
    List<ActionsInFrame> pendingActions = new List<ActionsInFrame>();
    bool positionChanged = false;
    Vector3 lastPosition;
    //bool rotationChanged = false;
    //private Quaternion lastRotation;

    [SerializeField] float sensitivity = 20.0f;
    [SerializeField] PlayerCamera playerCam;
    [SerializeField] GameObject camPivot;

    private void Awake()
    {
        playerCam = Camera.main.GetComponent<PlayerCamera>();
    }

    private void Update()
    {
        if(isOwner)
        {
            HandleInput();
            if (Input.GetKeyDown(KeyCode.K))
            {
                Instantiate(Resources.Load("Prefabs/BasicEnemyPrefab"));
            }
        }
    }

    void HandleInput()
    {
        List<PlayerAction> actionsInFrame = new List<PlayerAction>();

        if (Input.GetKey(KeyCode.W))
        {
            actionsInFrame.Add(new PlayerAction(PlayerAction.ActionType.MoveF));
        }
        if (Input.GetKey(KeyCode.S))
        {
            actionsInFrame.Add(new PlayerAction(PlayerAction.ActionType.MoveB));
        }
        if (Input.GetKey(KeyCode.A))
        {
            actionsInFrame.Add(new PlayerAction(PlayerAction.ActionType.MoveL));
        }
        if (Input.GetKey(KeyCode.D))
        {
            actionsInFrame.Add(new PlayerAction(PlayerAction.ActionType.MoveR));
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (mouseX != 0 || mouseY != 0)
        {
            actionsInFrame.Add(new PlayerAction(PlayerAction.ActionType.Rotate, new List<string>() { (mouseX * sensitivity).ToString(), 
                                                                                                     (mouseY * sensitivity).ToString() }));
        }

        if (actionsInFrame.Count > 0)
        {
            ActionsInFrame actionsToExecute = new ActionsInFrame(actionsInFrame, Time.deltaTime);

            ExecuteActions(actionsToExecute);

            pendingActions.Add(actionsToExecute);
        }
    }

    public void ExecuteActions(ActionsInFrame actions)
    {
        foreach (var action in actions.actionsInOneFrame)
        {
            switch (action.actionType)
            {
                case PlayerAction.ActionType.MoveF:
                case PlayerAction.ActionType.MoveB:
                case PlayerAction.ActionType.MoveL:
                case PlayerAction.ActionType.MoveR:
                    Move(action.actionType, actions.frameDeltaTime);
                    break;
                case PlayerAction.ActionType.Rotate:
                    Rotate(float.Parse(action.parameters[0]) * actions.frameDeltaTime, 
                           float.Parse(action.parameters[1]) * actions.frameDeltaTime);
                    break;
                case PlayerAction.ActionType.None:
                default:
                    break;
            }
        }

        
    }

    void Move(PlayerAction.ActionType action, float deltaTime)
    {
        Vector3 direction = Vector3.zero;

        switch (action)
        {
            case PlayerAction.ActionType.MoveF:
                direction = transform.forward;
                break;
            case PlayerAction.ActionType.MoveB:
                direction = -transform.forward;
                break;
            case PlayerAction.ActionType.MoveL:
                direction = -transform.right;
                break;
            case PlayerAction.ActionType.MoveR:
                direction = transform.right;
                break;
            case PlayerAction.ActionType.None:
            default:
                break;
        }

        if (direction.magnitude > 0)
        {
            transform.Translate(direction * moveSpeed * deltaTime, Space.World);

            if (transform.position != lastPosition) positionChanged = true;
            
        }
    }
    void Rotate(float xIncrement, float yIncrement)
    {
        transform.Rotate(Vector3.up * xIncrement);
        camPivot.transform.Rotate(Vector3.right * -yIncrement);
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

    public override NetInfo GetNetInfo()
    {
        return new Player(this);
    }
    //In the case of the player, this serves as state confirmation
    public override void UpdateObject(NetInfo info)
    {
        Wrappers.Player pw = (Wrappers.Player)info;

        transform.position = pw.position;
        transform.rotation = pw.rotation;
    }

    public void AttachCamera()
    {
        Camera.main.gameObject.GetComponent<PlayerCamera>().SetParent(camPivot.transform);
    }
}
