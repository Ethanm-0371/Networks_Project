using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] Vector3 offset;

    void Update()
    {
        //Update offset visual
        if (Input.GetKeyDown(KeyCode.T))
        {
            SetParent(transform.parent);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            offset.x *= -1;
            transform.localPosition = offset;
        }
    }

    public void SetParent(Transform parent)
    {
        transform.parent = parent;

        transform.localPosition = offset;
    }
}
