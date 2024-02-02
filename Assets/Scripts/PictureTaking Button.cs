using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static Codice.CM.Common.CmCallContext;

public class PictureTakingButton : MonoBehaviour
{

    
    bool takingNewPicture = false;
    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
    Resolution cameraResolution;
    public ARPlaneManager arPlaneManager;
    private List<Vector3> cablePositinos = new List<Vector3>();
    List<Vector2> redPixelCoordinates = new List<Vector2>();
    GameObject instantiatedObject;
    bool isObjectInstantiated = false;
    bool shouldMove = false;
    int showInformation = 0;
    private Transform camera;

    public ARRaycastManager raycastManager;

    [SerializeField]
    private GameObject virtualObject;

    public GameObject sendPackageButton;

    public GameObject scanningButton;

    public GameObject infoTextT;

    public GameObject infoTextS;

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
        Texture2D editedTexture = editTextureV3(targetTexture);

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
        if (showInformation == 0)
        {
            infoTextT.SetActive(true);
            infoTextT.transform.position = cablePositinos[1] + new Vector3(0, 0.10f, 0.1f);
            showInformation = 1;
        }
        else if (showInformation == 1)
        {
            infoTextT.SetActive(false);
            infoTextS.SetActive(true);
            infoTextS.transform.position = cablePositinos[1] + new Vector3(0, 0.10f, 0.1f);
            showInformation = 2;
        } else if (showInformation == 2)
        {
            infoTextT.SetActive(false);
            infoTextS.SetActive(false);
            showInformation = 0;
        }  
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

    Texture2D editTextureV3(Texture2D textureToEdit)
    {

        List<Vector2> redPixels = new List<Vector2>();
        Color[] pixels = textureToEdit.GetPixels();
        for (int x = 0; x < textureToEdit.width; x++)
        {
            ThreadPool.QueueUserWorkItem((state) => checkForRedPixel(pixels, redPixels, x, textureToEdit.width, textureToEdit.height));
        }
            if (redPixels.Count > 0)
            {
                List<Vector2> longestRedLine = new List<Vector2>();
                List<Vector2> currentRedLine = new List<Vector2>();
                currentRedLine.Add(redPixels[0]);
                redPixels.RemoveAt(0);
                while (redPixels.Count > 0)
                {
                    Vector2 lastPoint = currentRedLine[currentRedLine.Count - 1];
                    Vector2 nextPoint = redPixels[0];
                    if (lastPoint.x == nextPoint.x && (lastPoint.y + 1 == nextPoint.y || lastPoint.y - 1 == nextPoint.y))
                    {
                        currentRedLine.Add(nextPoint);
                        redPixels.RemoveAt(0);
                    }
                    else
                    {
                        if (currentRedLine.Count > longestRedLine.Count)
                        {
                            longestRedLine = new List<Vector2>(currentRedLine);
                        }
                        currentRedLine.Clear();
                        currentRedLine.Add(redPixels[0]);
                        redPixels.RemoveAt(0);
                    }
                }
                if (currentRedLine.Count > longestRedLine.Count)
                {
                    longestRedLine = new List<Vector2>(currentRedLine);
                }
                if (longestRedLine.Count > 0)
                {
                    cablePositinos.Add(new Vector3(longestRedLine[0].x, longestRedLine[0].y, 0));
                    cablePositinos.Add(new Vector3(longestRedLine[longestRedLine.Count - 1].x, longestRedLine[longestRedLine.Count - 1].y, 0));
                }
        }
        return textureToEdit;
    }

    void checkForRedPixel(Color[] pixels, List<Vector2> redPixels, int x, int textureWidth, int textureHeight)
    {
        for (int y = 0; y < textureHeight; y++)
        {
            int index = y * textureWidth + x;
            if (pixels[index].r > 0.55f && pixels[index].g < 0.5f && pixels[index].b < 0.5f)
            {
                redPixels.Add(new Vector2(x, y));
            }
        }
    }

        Texture2D editTextureV2(Texture2D textureToEdit)
    {
        Debug.Log("Width " + textureToEdit.width);
        Debug.Log("Hight " + textureToEdit.height);

        // Get the pixel data from the texture
        Color[] pixels = textureToEdit.GetPixels();

        int textureWidth = textureToEdit.width;
        int textureHeight = textureToEdit.height;

        List<Vector2> longestRedLine = new List<Vector2>();

        float redThreshold = 0.55f; 

        int longestLineLength = 0;
        List<Vector2> currentRedLine = new List<Vector2>();

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                int index = y + x;


                if (pixels[index].r > redThreshold && pixels[index].g < 0.5f && pixels[index].b < 0.5f)
                {
                    currentRedLine.Add(new Vector2(x, y));
                    int currentX = x;
                    while (currentX < textureWidth - 1)
                    {
                        currentX++;
                        int newIndex = y + currentX;
                        if (pixels[newIndex].r > redThreshold && pixels[newIndex].g < 0.5f && pixels[newIndex].b < 0.5f)
                        {
                            currentRedLine.Add(new Vector2(currentX, y));
                        }
                        else
                        {
                            if(!checkForBlackThreshold(50,currentX, y, redThreshold, textureWidth, pixels))
                            {
                                break;// TODO Check if line below or above has red in same y
                            }
                        }
                    }                    

                    // Update longest line if current is longer
                    if (currentRedLine.Count > longestLineLength)
                    {
                        longestLineLength = currentRedLine.Count;
                        longestRedLine = new List<Vector2>(currentRedLine);
                    }

                    // Reset current line tracking
                    currentRedLine.Clear();
                }
                else
                {
                    pixels[index] = Color.black; // Set non-red pixels to black
                }
            }
        }

        if (longestRedLine.Count == 0)
        {
            Debug.Log("No Red Pixel detected.");
            return textureToEdit;
        }


        
        Debug.Log("longestRedLine: " + longestRedLine.Count);
        Debug.Log("longestRedLine[0] " + longestRedLine[0].y + " " + longestRedLine[0].x);
        redPixelCoordinates.Add(longestRedLine[0]);
        Debug.Log("longestRedLine[longestRedLine.Count / 2 " + longestRedLine[longestRedLine.Count / 2].y + " " + longestRedLine[longestRedLine.Count / 2].x);
        redPixelCoordinates.Add(longestRedLine[longestRedLine.Count / 2]);
        Debug.Log("longestRedLine[longestRedLine.Count / 2 " + longestRedLine[longestRedLine.Count - 1].y + " " + longestRedLine[longestRedLine.Count - 1].x);
        redPixelCoordinates.Add(longestRedLine[longestRedLine.Count - 1]);
        Debug.Log("Lenght Red: " + redPixelCoordinates.Count);
        // Apply the edited pixels back to the texture
        textureToEdit.SetPixels(pixels);
        textureToEdit.Apply();
        return textureToEdit;
    }

    bool checkForBlackThreshold(int nonRedThreshold, int currentX,int currenty, float redThreshold,int textureWidth, Color[] pixels)
    {
        int nonRedCount = 0;
        while (nonRedCount < nonRedThreshold && currentX < textureWidth - 1)
        {
            currentX++;
            int newNewIndex = currenty + currentX;
            if (!(pixels[newNewIndex].r > redThreshold && pixels[newNewIndex].g < 0.5f && pixels[newNewIndex].b < 0.5f))
            {
                nonRedCount++;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    /*
    Texture2D editTextureV2(Texture2D textureToEdit)
    {
        Debug.Log("Time beginning: " + Time.time);
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
                if (pixels[index].r > 0.55f && pixels[index].g < 0.5f && pixels[index].b < 0.5f)
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
            Debug.Log("No Red Pixel detected.");
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

        Debug.Log("Time End: " + Time.time);
        return textureToEdit;
    }*/

    Vector2 getSideCoordinate(List<int> list,bool min, int averageY)
    {
        list.Sort();
        int sumOfSelectedValues = 0;
        if (min)
        {
            for (int i = 0; i < list.Count/5; i++) 
            {
                sumOfSelectedValues += list[i];
            }
        } 
        else
        {
            int cal = list.Count - (list.Count / 5);
            for (int i = 0; i < list.Count / 5; i++)
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
        if (!shouldMove && isObjectInstantiated)
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
        Debug.Log("Jo bin da");


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
                            cablePositinos.Add(new Vector3(hits[i].pose.position.x, hits[i].pose.position.y + 0.05f, hits[i].pose.position.z));
                            debugRaycast(hits[i], Color.red);
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
}
