using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class InventoryPlacementController : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public GameObject inventoryObject;
    public InventoryController inventoryController;
    public GameObject qrCodesManager;
    public GameObject infoObject;
    public float requiredLookTime = 5.0f; 
    public Vector3 objectPosition;

    private ARPlane selectedDeskPlane;
    private float lookStartTime = -1f;
    private bool objectPlaced = false;
    private float heightOffset = 0.001f;

    private bool canStartScript = false;

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(3.0f);
        canStartScript = true;
    }

    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    // The following commented functions are old versions of the code
    /*
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
    */

    /*
    private ARPlane FindClosestPlane(List<ARRaycastHit> hits)
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
    */

    void Update()
    {
        if (!objectPlaced && canStartScript)
        {
            Debug.Log("In Update! 1");
            if (IsPointerOverPlane())
            {
                Debug.Log("In Update! 2");
                ARPlane currentPlane = GetCurrentPlaneUnderGaze();

                if (currentPlane != null)
                {
                    Debug.Log("In Update! 3");
                    if (selectedDeskPlane == null || selectedDeskPlane != currentPlane)
                    {
                        Debug.Log("In Update! 4");
                        SetPlaneColor(currentPlane, Color.red); // Change plane color to red
                        selectedDeskPlane = currentPlane;
                        lookStartTime = Time.time;
                        Debug.Log("Looked at plane for: " + lookStartTime.ToString());
                    }
                    Debug.Log("In Update! 5");
                    float timeLookedAtPlane = Time.time - lookStartTime;
                    if (timeLookedAtPlane >= requiredLookTime)
                    {
                        Debug.Log("In Update! 6");
                        PlaceObjectOnDesk(selectedDeskPlane);
                        objectPlaced = true;
                    }
                }
                else
                {
                    Debug.Log("In Update! 7");
                    if (selectedDeskPlane != null)
                    {
                        SetPlaneColor(selectedDeskPlane, Color.blue); // Reset previous plane color
                    }
                    selectedDeskPlane = null;
                }
            }
            else
            {
                Debug.Log("In Update! 8");
                if (selectedDeskPlane != null)
                {
                    SetPlaneColor(selectedDeskPlane, Color.blue); // Reset previous plane color
                }
                selectedDeskPlane = null;
            }
        }
    }

    bool IsPointerOverPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            ARPlane plane = hit.collider.GetComponent<ARPlane>();
            Debug.Log("Plane info in isPointerOverPlane: " + plane.trackableId.ToString() + " " + plane.alignment.ToString() + " " + plane.center.ToString() + " " + plane.extents.ToString());  
            return (plane != null);
        }

        return false;
    }

    ARPlane GetCurrentPlaneUnderGaze()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            ARPlane plane = hit.collider.GetComponent<ARPlane>();
            Debug.Log("Plane info in GetCurrentPlaneUnderGaze: " + plane.trackableId.ToString() + " " + plane.alignment.ToString() + " " + plane.center.ToString() + " " + plane.extents.ToString());
            return plane;
        }

        return null;
    }

    //For Debugging purposes
    void SetPlaneColor(ARPlane plane, Color color)
    {
        // Assuming ARPlane visualizer is a child GameObject with a MeshRenderer component
        MeshRenderer planeRenderer = plane.GetComponentInChildren<MeshRenderer>();
        if (planeRenderer != null)
        {
            planeRenderer.material.color = color;
        }
    }

    private void PlaceObjectOnDesk(ARPlane deskPlane)
    {
        Debug.Log("Inside PlaceObjectOnDesk!");

        qrCodesManager.SetActive(true);
        objectPosition = deskPlane.center + Vector3.up * heightOffset;
        Quaternion objectRotation = Quaternion.Euler(-90f, 0f, 0f);
        GameObject instantiatedObject = Instantiate(inventoryObject, objectPosition, objectRotation);
        instantiatedObject.transform.localScale = new Vector3(20f, 20f, 20f);
        Vector3 infoObjectPosition = objectPosition - Vector3.forward * 4.415f + Vector3.right * 0.4f;
        infoObject.transform.position = infoObjectPosition;
        infoObject.SetActive(true);
        inventoryController.SetInventoryObject(instantiatedObject);
        inventoryController.gameObject.SetActive(true);
        planeManager.planePrefab.SetActive(false);
        gameObject.SetActive(false);
    }
}