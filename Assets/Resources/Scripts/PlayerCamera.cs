using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    Transform pivot;

    private void Awake()
    {
        pivot = transform.parent;
    }

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
        pivot.parent = parent;

        pivot.position = parent.transform.position + Vector3.up;

        transform.localPosition = offset;
    }
}
