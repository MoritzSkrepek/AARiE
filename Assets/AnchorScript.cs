using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceObjectOnLookedAtDesk : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public ARAnchorManager anchorManager;     //TODO: add the Anchor to the placed Object
    public GameObject objectToPlace; // Assign your 3D object in the inspector.
    public float requiredLookTime = 3.0f; // Time in seconds for desk confirmation.

    private ARPlane selectedDeskPlane;
    private float lookStartTime = -1f;
    private bool objectPlaced = false;
    private float heightOffset = 0.05f;

    void Update()
    {
        if (!objectPlaced)
        {
            Debug.Log("Object has not been placed yet!");

            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes))
            {
                Debug.Log("Player is looking at a plane");
                ARPlane plane = planeManager.GetPlane(hits[0].trackableId);
                if (plane != null)
                {
                    if (selectedDeskPlane == null)
                    {
                        selectedDeskPlane = plane;
                        lookStartTime = Time.time; // Start the timer when a new plane is selected.
                        Debug.Log("Plane selected. Timer started.");
                    }

                    if (selectedDeskPlane == plane)
                    {
                        float timeLookedAtPlane = Time.time - lookStartTime;
                        Debug.Log("Time looked at plane: " + timeLookedAtPlane);

                        if (timeLookedAtPlane >= requiredLookTime)
                        {
                            PlaceObjectOnDesk(selectedDeskPlane);
                            Debug.Log("Object is now placed.");
                            objectPlaced = true;
                        }
                    }
                    else
                    {
                        Debug.Log("User is now looking at a different plane.");
                        selectedDeskPlane = null;
                    }
                }
                else
                {
                    Debug.Log("User is now looking at a different plane.");
                    selectedDeskPlane = null;
                }
            }
            else
            {
                Debug.Log("No planes detected.");
                selectedDeskPlane = null;
            }
        }
    }

    void PlaceObjectOnDesk(ARPlane deskPlane)
    {
        // Disable the plane manager to stop further plane detection.
        planeManager.enabled = false;

        // Disable this script so it won't run again.
        gameObject.SetActive(false);

        // Calculate the object's position above the center of the plane.
        Vector3 objectPosition = deskPlane.center + Vector3.up * heightOffset;

        // Instantiate the object and place it at the calculated position.
        Instantiate(objectToPlace, objectPosition, Quaternion.identity);

        /*
        //This code handles the calculation for the placement position and also attaches an ARAnchor to the placed Object
        // Disable the plane manager to stop further plane detection.
        planeManager.enabled = false;

        // Disable this script so it won't run again.
        gameObject.SetActive(false);

        // Calculate the object's position above the center of the plane.
        Vector3 objectPosition = deskPlane.center + Vector3.up * heightOffset;

        //Create Anchor
        ARAnchor newAnchor = anchorManager.AddComponent<ARAnchor>();
        GameObject anchorVisual = Instantiate(objectToPlace, objectPosition, Quaternion.identity);
        anchorVisual.transform.parent = newAnchor.transform;
        */
    }
}

