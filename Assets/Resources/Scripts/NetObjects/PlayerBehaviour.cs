using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wrappers;

public class PlayerBehaviour : NetObject
{
    public bool isOwner = false;

    private void Update()
    {
        if(isOwner)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        float moveSpeed = 5f;

        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized;

        if (move.magnitude > 0)
        {
            transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    public override void UpdateObject(object info)
    {
        PlayerWrapper pw = (PlayerWrapper)info;

        transform.position = pw.p;
        transform.rotation = pw.r;
    }
}
