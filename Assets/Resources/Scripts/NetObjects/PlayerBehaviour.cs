using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wrappers;

public class PlayerBehaviour : NetObject
{
    public bool isOwner = false;
    public string ownerName;
    float moveSpeed = 5f;

    // Last state variables
    bool actionsChanged = false;
    List<ActionsInFrame> pendingActions = new List<ActionsInFrame>();
    bool positionChanged = false;
    Vector3 lastPosition;
    //bool rotationChanged = false;
    //private Quaternion lastRotation;

    [SerializeField] float sensitivity = 100.0f;
    [SerializeField] float verticalRotation = 0f;
    [SerializeField] const float maxAngle = 90f;
    [SerializeField] PlayerCamera playerCam;
    [SerializeField] Transform gunPivot;
    public Transform camPivot;
    public bool lockCamera = false;

    Gun currentGun;

    private void Awake()
    {
        playerCam = Camera.main.GetComponent<PlayerCamera>();

        //Testing befor having Level1. This should not go here
        GameObject gunGO = (GameObject)Instantiate(Resources.Load("Prefabs/Gun"), gunPivot);
        currentGun = gunGO.GetComponent<Gun>();
    }

    private void Update()
    {
        if (isOwner)
        {
            HandleInput();
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (FindDoor()) actionsInFrame.Add(new PlayerAction(PlayerAction.ActionType.OpenDoor));
        }

        float mouseX = 0;
        float mouseY = 0;

        if (!lockCamera)
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }

        if (mouseX != 0 || mouseY != 0)
        {
            actionsInFrame.Add(new PlayerAction(PlayerAction.ActionType.Rotate, new List<string>() { (mouseX * sensitivity).ToString(),
                                                                                                     (mouseY * sensitivity).ToString() }));
        }

        if (Input.GetButtonDown("Fire1"))
        {
            actionsInFrame.Add(currentGun.CalculateShot());
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
                case PlayerAction.ActionType.Shot:
                    ShootCurrentGun(action);
                    break;
                case PlayerAction.ActionType.OpenDoor:
                    OpenDoor();
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

        verticalRotation += -yIncrement;

        verticalRotation = Mathf.Clamp(verticalRotation, -maxAngle, maxAngle);

        camPivot.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
    void ShootCurrentGun(PlayerAction shotAction)
    {
        Vector3 origin = new Vector3(float.Parse(shotAction.parameters[0]), float.Parse(shotAction.parameters[1]), float.Parse(shotAction.parameters[2]));
        Vector3 direction = new Vector3(float.Parse(shotAction.parameters[3]), float.Parse(shotAction.parameters[4]), float.Parse(shotAction.parameters[5]));

        currentGun.Shoot(origin, direction);
    }
    void OpenDoor()
    {
        GameObject.Find("SafeRoomDoor").GetComponentInChildren<SafeRoomDoor>()?.Open(transform.position); //This hurts a lot, please find a better way.
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
    }

    public void InitPlayer(Wrappers.Player info)
    {
        ownerName = info.o;

        if (ownerName != GameClient.Singleton.userName)
        {
            GetComponentInChildren<TMPro.TextMeshProUGUI>().text = ownerName;

            camPivot.rotation = info.camPivot;

            return;
        }

        isOwner = true;
        GameClient.Singleton.ownedPlayerGO = gameObject;

        AttachCamera();

        GetComponentInChildren<Canvas>().gameObject.SetActive(false);
    }

    public void AttachCamera()
    {
        Camera.main.gameObject.GetComponent<PlayerCamera>().SetParent(camPivot);
        Camera.main.transform.rotation = transform.rotation;
    }

    bool FindDoor()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit reticleTarget, 5f))
        {
            Debug.Log($"Hit: {reticleTarget.collider.gameObject.name}");

            if (reticleTarget.collider.GetComponent<SafeRoomDoor>())
            {
                return true;
            }
        }
        return false;
    }
}
