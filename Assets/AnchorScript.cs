using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceObjectOnLookedAtDesk : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public GameObject inventoryObject;
    public GameObject testObject;
    public InventoryController inventoryController;
    public GameObject qrCodesManager;
    public GameObject infoObject;
    public float requiredLookTime = 5.0f; // Time in seconds for desk confirmation.

    private ARPlane selectedDeskPlane;
    private float lookStartTime = -1f;
    private bool objectPlaced = false;
    private float heightOffset = 0.05f;

    private bool canStartScript = false;

    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(3.0f); // Adjust the delay time as needed.
        canStartScript = true;
    }

    void Update()
    {
        if (!objectPlaced && canStartScript)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            // Use Camera.main.transform.forward as the ray direction
            if (raycastManager.Raycast(new Ray(Camera.main.transform.position, Camera.main.transform.forward), hits, TrackableType.Planes))
            {
                ARPlane closestPlane = FindClosestPlane(hits);
                if (closestPlane != null)
                {
                    if (selectedDeskPlane == null || selectedDeskPlane != closestPlane)
                    {
                        selectedDeskPlane = closestPlane;
                        lookStartTime = Time.time; // Start the timer when a new plane is selected.
                    }
                    float timeLookedAtPlane = Time.time - lookStartTime;
                    if (timeLookedAtPlane >= requiredLookTime)
                    {
                        PlaceObjectOnDesk(selectedDeskPlane);
                        objectPlaced = true;
                    }
                }
                else
                {
                    selectedDeskPlane = null;
                }
            }
            else
            {
                selectedDeskPlane = null;
            }
        }
    }

    ARPlane FindClosestPlane(List<ARRaycastHit> hits)
    {
        ARPlane closestPlane = null;
        float closestDistance = float.MaxValue;
        foreach (var hit in hits)
        {
            ARPlane plane = planeManager.GetPlane(hit.trackableId);
            if (plane != null)
            {
                float distanceToPlane = Vector3.Distance(Camera.main.transform.position, hit.pose.position);
                if (distanceToPlane < closestDistance)
                {
                    closestPlane = plane;
                    closestDistance = distanceToPlane;
                }
            }
        }
        return closestPlane;
    }

    void PlaceObjectOnDesk(ARPlane deskPlane)
    {
        // Calculate the object's position above the center of the plane.
        Vector3 objectPosition = deskPlane.center + Vector3.up * heightOffset;
        // Calculate the rotation to rotate the object -90 degrees around the x-axis.
        Quaternion objectRotation = Quaternion.Euler(-90f, 0f, 0f);
        // Instantiate the object with rotation.
        GameObject instantiatedObject = Instantiate(inventoryObject, objectPosition, objectRotation);
        // Set the scale of the instantiated object.
        instantiatedObject.transform.localScale = new Vector3(30f, 30f, 30f);
        // Spawn infoGameObject (Two TextMeshes and button for Knapsack Algorithm)
        Vector3 infoObjectPosition = objectPosition - Vector3.forward * 5f;
        infoObject.transform.position = infoObjectPosition;
        infoObject.SetActive(true);
        // Set the inventoryObject in the InventoryController
        inventoryController.SetInventoryObject(instantiatedObject);
        // Enable the InventoryController
        inventoryController.gameObject.SetActive(true);
        // Set the visibility of the planes.

        Debug.Log("Coords: " + instantiatedObject.transform.position);
        Debug.Log("Cords; " + infoObject.transform.position);
        
        planeManager.planePrefab = null;
        planeManager.enabled = false;
        qrCodesManager.SetActive(true);
        // Disable this script so it won't run again.
        this.gameObject.SetActive(false);
    }
}
