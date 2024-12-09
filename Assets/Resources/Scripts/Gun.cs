using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] Transform gunMuzzle;
    Vector3 debugHit = Vector3.zero;

    private void Update()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit raycasthit, 999f))
        {
            debugHit = raycasthit.point;
        }
        else
        {
            debugHit = gunMuzzle.forward * 20f;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("Pium");
        }
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(gunMuzzle.position, debugHit, Color.green);
        Debug.DrawLine(gunMuzzle.position, gunMuzzle.forward * 20f, Color.red);
    }
}
