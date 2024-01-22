using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PictureTakingButton : MonoBehaviour
{

    
    bool takingNewPicture = false;
    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
    Resolution cameraResolution;
    private float redComponentThreshold = 0.7f;
    public ARPlaneManager arPlaneManager;
    private List<Vector3> cablePositinos = new List<Vector3>();
    List<Vector2> redPixelCoordinates = new List<Vector2>();
    GameObject instantiatedObject;
    bool isObjectInstantiated = false;
    bool shouldMove = false;
    bool showInformation = false;
    private Transform camera;

    public ARRaycastManager raycastManager;

    [SerializeField]
    private GameObject virtualObject;

    public GameObject sendPackageButton;

    public GameObject scanningButton;

    public GameObject infoObject;

    // Use this for initialization
    void Start()
    {
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        camera = Camera.main.transform;
    }
    public void takingPicture()
    {
        if (!takingNewPicture)
        {
            Debug.Log("Taking");
            redPixelCoordinates.Clear();
            takingNewPicture = true;

            PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject)
            {
                if (captureObject != null)
                {
                    photoCaptureObject = captureObject;
                    CameraParameters cameraParameters = new CameraParameters();
                    cameraParameters.hologramOpacity = 0.0f;
                    cameraParameters.cameraResolutionWidth = cameraResolution.width;
                    cameraParameters.cameraResolutionHeight = cameraResolution.height;
                    cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
                    try
                    {

                        photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result)
                        {
                            photoCaptureObject.TakePhotoAsync(onCapturedPhotoToMemory);
                        });

                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }
                }
            });
        }
    }

    void onCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        // Edit the texture
        Texture2D editedTexture = editTextureV2(targetTexture);

        // Set the edited texture to the material of the Renderer
        Renderer rend = GetComponent<Renderer>();
        rend.material.mainTexture = editedTexture;

        // Deactivate the camera
        photoCaptureObject.StopPhotoModeAsync(onStoppedPhotoMode);
    }

    void onStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown the photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;

        if (redPixelCoordinates.Count != 0)
        {

            foreach (Vector2 redPixle in redPixelCoordinates)
            {
                if(PositionVirtualObject(redPixle, targetTexture))
                {
                    sendPackageButton.SetActive(true);
                    scanningButton.SetActive(false);
                }
            }
        }
        takingNewPicture = false;
    }

    public void ShowInformation()
    {
        showInformation = !showInformation;
        infoObject.SetActive(showInformation);
        infoObject.transform.position = cablePositinos[1] + new Vector3(0,0.10f,0.1f);
    }

    public void ShowAndSendPackage()
    {
        if (isObjectInstantiated == false)
        {
            instantiatedObject = Instantiate(virtualObject, cablePositinos[0], Quaternion.identity);
            isObjectInstantiated = true;
        } else
        {
            Destroy(instantiatedObject);
            instantiatedObject = Instantiate(virtualObject, cablePositinos[0], Quaternion.identity);
        }
        shouldMove = !shouldMove;
    }

    Texture2D editTextureV2(Texture2D textureToEdit)
    {
        // Get the pixel data from the texture
        Color[] pixels = textureToEdit.GetPixels();

        int textureWidth = textureToEdit.width;

        List<int> yCoordinates = new List<int>();
        List<int> xCoordinates = new List<int>();

        for (int y = 0; y < textureToEdit.height; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                int index = y * textureWidth + x;

                // Check if the pixel is predominantly red (you can adjust the threshold)
                if (pixels[index].r > redComponentThreshold && pixels[index].g < 0.5f && pixels[index].b < 0.5f)
                {
                    pixels[index] = Color.red;

                    yCoordinates.Add(y);
                    xCoordinates.Add(x);
                }
                else
                {
                    pixels[index] = Color.black; // Set non-red pixels to black
                }
            }
        }

        if ( yCoordinates.Count == 0 || xCoordinates.Count == 0 )
        {
            return textureToEdit;
        }
        // Calculate average X coordinate
        int averageX = getCenterCoordinate(xCoordinates);
        int averageY = getCenterCoordinate(yCoordinates);
        // Add the min X, average Y to redPixelCoordinates
        redPixelCoordinates.Add(getSideCoordinate(xCoordinates,true, averageY));
        // Add the average X, average Y to redPixelCoordinates
        redPixelCoordinates.Add(new Vector2(averageX, averageY));
        // Add the max X, average Y to redPixelCoordinates
        redPixelCoordinates.Add(getSideCoordinate(xCoordinates, false, averageY));

        // Apply the edited pixels back to the texture
        textureToEdit.SetPixels(pixels);
        textureToEdit.Apply();

        return textureToEdit;
    }

    Vector2 getSideCoordinate(List<int> list,bool min, int averageY)
    {
        list.Sort();
        int sumOfSelectedValues = 0;
        if (min)
        {
            for (int i = 0; i < list.Count/10; i++) 
            {
                sumOfSelectedValues += list[i];
            }
        } 
        else
        {
            int cal = list.Count - (list.Count / 10);
            for (int i = 0; i < list.Count / 10; i++)
            {
                sumOfSelectedValues += list[cal + i];
            }
        }
        return new Vector2(sumOfSelectedValues / (list.Count / 10), averageY);
    }

    int getCenterCoordinate(List<int> list)
    {
        int min = list.Min();
        int max = list.Max();
        return Mathf.RoundToInt((min+max) / 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldMove)
        {
            instantiatedObject.transform.Translate((cablePositinos[2] - cablePositinos[0]) * 0.02f);
            if (Vector3.Distance(instantiatedObject.transform.position, cablePositinos[2]) < 0.01f)
                {
                instantiatedObject.transform.position = cablePositinos[2];

                shouldMove = !shouldMove;
            }
        }
        if (!shouldMove && isObjectInstantiated == true)
        {
            instantiatedObject.transform.LookAt(camera);
        }
    }

    ARPlane FindClosestPlane(List<ARRaycastHit> hits)
    {
        ARPlane closestPlane = null;
        float closestDistance = float.MaxValue;
        foreach (var hit in hits)
        {
            ARPlane plane = arPlaneManager.GetPlane(hit.trackableId);
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
    bool PositionVirtualObject(Vector2 redPixel, Texture2D image)
    {
        Vector2 screenCoordinates = new Vector2(
            redPixel.x * Camera.main.pixelWidth / image.width,
            redPixel.y * Camera.main.pixelHeight / image.height
        );


        //(Debug) For comparison of the screen coordinates and the red pixel coordinates    
        //
        //Debug.Log("screencords: " + screenCoordinates.x + " " + screenCoordinates.y);
        //Debug.Log("redpixel: " + redPixel.x + " " + redPixel.y);
        //Debug.Log("image size: " + image.width + " " + image.height);

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        
        if (raycastManager.Raycast(screenCoordinates, hits, TrackableType.Planes))
        { 
            for (int i = 0; i < hits.Count; i++)
                {
                    if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
                    {
                        if (hits[i] != null)
                        {
                            cablePositinos.Add(new Vector3(hits[i].pose.position.x, hits[i].pose.position.y + 0.25f, hits[i].pose.position.z));
                            //debugRaycast(hits[i], Color.red);
                        break;
                        } else
                        {
                            Debug.LogWarning("Hit " + hits[i] + " is null");
                        return false;
                        }
                    }
            }
            return true;
        } else
        {
            Debug.LogWarning("No Hits");
            return false;
        }

    }



    private List<GameObject> instantiatedObjects = new List<GameObject>();

    void debugRaycast(ARRaycastHit hit, Color color)
    {

        Debug.Log("Ray hit position: " + hit.pose.position);

        GameObject instantiatedObject = Instantiate(virtualObject, hit.pose.position, Quaternion.identity);

        // Keep track of the instantiated objects
        instantiatedObjects.Add(instantiatedObject);
    }

    void debugRaycast(Vector3 hit, Color color)
    {

        Debug.Log("Ray hit position: " + hit);

        GameObject instantiatedObject = Instantiate(virtualObject, hit, Quaternion.identity);

        // Keep track of the instantiated objects
        instantiatedObjects.Add(instantiatedObject);
    }
}
