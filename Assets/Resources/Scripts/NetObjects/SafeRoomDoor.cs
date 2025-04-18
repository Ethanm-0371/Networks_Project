using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeRoomDoor : MonoBehaviour
{
    [SerializeField] float openDistance = 2f;
    bool opening = false;
    bool open = false;

    [SerializeField] float rotationSpeed = 10f;

    void Update()
    {
        if (opening) 
        {
            if (transform.localRotation.eulerAngles.y < -165f) { opening = false; }
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, -170, 0), rotationSpeed * Time.deltaTime);
        }
    }

    public void Open(Vector3 playerPosition)
    {
        float distance = Vector3.Distance(playerPosition, transform.position);

        if (!open && distance < openDistance) 
        {
            open = true;
            opening = true;

            GameServer.Singleton?.SetLevelStatus(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, openDistance);
    }
}
