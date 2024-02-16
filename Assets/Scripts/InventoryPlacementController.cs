using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class InventoryPlacementController : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public GameObject inventoryObject;
    public InventoryController inventoryController;
    public GameObject qrCodesManager;
    public GameObject infoObject;
    public TextMeshPro userInfo;
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
        userInfo.text = "Schauen Sie auf einen Tisch";
        StartCoroutine(DelayedStart());
    }

    void Update()
    {
        if (!objectPlaced && canStartScript)
        {
            if (IsPointerOverPlane())
            {
                ARPlane currentPlane = GetCurrentPlaneUnderGaze();

                if (currentPlane != null)
                {
                    if (selectedDeskPlane == null || selectedDeskPlane != currentPlane)
                    {
                        selectedDeskPlane = currentPlane;
                        lookStartTime = Time.time;
                    }
                    float timeLookedAtPlane = Time.time - lookStartTime;
                    userInfo.text = ((int)requiredLookTime - (int)timeLookedAtPlane).ToString();
                    if (timeLookedAtPlane >= requiredLookTime)
                    {
                        PlaceObjectOnDesk(selectedDeskPlane);
                        objectPlaced = true;
                        userInfo.text = "";
                    }
                }
                else
                {
                    selectedDeskPlane = null;
                    userInfo.text = "Schauen Sie auf einen Tisch";
                }
            }
            else
            {
                selectedDeskPlane = null;
                userInfo.text = "Schauen Sie auf einen Tisch";
            }
        }
    }

    private bool IsPointerOverPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            ARPlane plane = hit.collider.GetComponent<ARPlane>();
            return (plane != null);
        }
        return false;
    }

    private ARPlane GetCurrentPlaneUnderGaze()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            ARPlane plane = hit.collider.GetComponent<ARPlane>();
            return plane;
        }
        return null;
    }

    private void PlaceObjectOnDesk(ARPlane deskPlane)
    {
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
