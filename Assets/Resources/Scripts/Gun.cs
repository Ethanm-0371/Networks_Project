using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] Transform gunMuzzle;
    Vector3 debugHit = Vector3.zero;
    Vector3 reticleDebugHit = Vector3.zero;
    Vector3 furtherReticleDebugHit = Vector3.zero;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            CalculateShot();
        }
    }

    void CalculateShot()
    {
        //Create the ray forward from the camera
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        //Create the gun plane
        Plane gunPlane = new Plane(gunMuzzle.forward, gunMuzzle.position);

        //Shoot rays FROM CAMERA for the target and for the plane
        if (Physics.Raycast(ray, out RaycastHit reticleTarget, 999f) &&
            gunPlane.Raycast(ray, out float distanceToPlane))
        {
            reticleDebugHit = reticleTarget.point;

            if (distanceToPlane < reticleTarget.distance) //check if the hit object is further that the hit on the gun plane
            {
                Debug.Log("Shooting to reticle");
                DoShot(reticleTarget.point);
            }
            else
            {
                if (Physics.Raycast(reticleTarget.point, ray.direction, out RaycastHit targetPastReticle, 999f)) //shoot the same ray but with the previously hit point as origin
                {
                    Debug.Log("Shooting past reticle");
                    furtherReticleDebugHit = targetPastReticle.point;
                    DoShot(targetPastReticle.point);
                }
                else { Debug.Log("Shooting to infinite"); }
            }
        }
        else { Debug.Log("Shooting to infinite"); }
    }

    void DoShot(Vector3 bulletTarget)
    {
        if (Physics.Raycast(gunMuzzle.position, bulletTarget - gunMuzzle.position, out RaycastHit bulletHit, 999f))
        {
            debugHit = bulletHit.point; //This is where the bullet must land
            //Debug.Log($"Bullet hit {bulletHit.collider.gameObject.name}");
        }
        else { Debug.LogError("Bullet path hit nothing. You should not see this message. If you do, something has gone really bad"); }
    }


    private void OnDrawGizmos()
    {
        //Green is muzzle to hitPoint
        Debug.DrawLine(gunMuzzle.position, debugHit, Color.green);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(reticleDebugHit, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(furtherReticleDebugHit, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(debugHit, 0.05f);
    }
}
