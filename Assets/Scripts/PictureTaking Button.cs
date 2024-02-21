using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Debug = UnityEngine.Debug;


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
    public bool planeInUse = false;

    private Transform camera;

    public ARRaycastManager raycastManager;

    [SerializeField]
    private GameObject virtualObject;

    public GameObject sendPackageButton;

    public GameObject scanningButton;

    public GameObject infoTextT;

    public GameObject infoTextS;

    public GameObject loadingCircle;
    private Canvas loadingCircleCanvas;

    void Start()
    {
        //EventManager.OnMessageReceived += SendMessage;
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).FirstOrDefault();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        camera = Camera.main.transform;
    }
    
public void takingPicture()
    {
        if (!takingNewPicture)
        {
            loadingCircle.SetActive(true);
            loadingCircleCanvas = loadingCircle.GetComponentInChildren<Canvas>();
            loadingCircleCanvas.GetComponent<LoadingCircle>().StartLoading();
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
                        Debug.LogWarning("Error at StartPhotoModeAsync \n" + e);
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
        if (planeInUse)
        {
            Renderer rend = GetComponent<Renderer>();
            rend.material.mainTexture = editedTexture;
        }

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
                    //scanningButton.SetActive(false);
                }
            }

        }
        takingNewPicture = false;
        loadingCircleCanvas.GetComponent<LoadingCircle>().StopLoading();
        loadingCircle.SetActive(false);
    }

    public void ShowInformation()
    {
        if (showInformation == 0)
        {
            infoTextT.SetActive(true);
            infoTextT.transform.position = cablePositinos[(cablePositinos.Count - 1) / 2] + new Vector3(0, 0.10f, 0.2f);
            showInformation = 1;
        }
        else if (showInformation == 1)
        {
            infoTextT.SetActive(false);
            infoTextS.SetActive(true);
            infoTextS.transform.position = cablePositinos[(cablePositinos.Count-1)/2] + new Vector3(0, 0.10f, 0.2f);
            showInformation = 2;
        } else if (showInformation == 2)
        {
            infoTextT.SetActive(false);
            infoTextS.SetActive(false);
            showInformation = 0;
        }  
    }

    public void ShowAndSendPackage()//string username, string message
    {

        if (isAtEnd)
        {
            isAtEnd = false;
            moveToCounter = 1;
            moveFromCounter = 0;

        }
        //Debug.Log("username: " + username + " m: "+ message);
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

    private bool[,] redPixels;
    Texture2D editTextureV3(Texture2D textureToEdit)
    {        
        List<Vector2> longestRedLine = new List<Vector2>();
        PixelColorJob pixelColorJobInstance = new PixelColorJob();
        pixelColorJobInstance.RedPixelSearch(textureToEdit);
        redPixels = pixelColorJobInstance.redPixleList;

        for (int x = 0; x < redPixels.GetLength(0); x++)
        {

            for (int y = 0; y < redPixels.GetLength(1); y++)
            {
                if (redPixels[x,y])
                {
                    List<Vector2> currentRedLine = new List<Vector2>();
                    currentRedLine = SearchForRedPixels(x, y);
                    if (currentRedLine.Count > longestRedLine.Count)
                    {
                        longestRedLine = currentRedLine;
                    }
                    break;
                }
            }
        }

        Debug.Log(longestRedLine.Count());
        if (longestRedLine.Count() > 0) 
        {
            addPointsForRaycast(longestRedLine);
        }

        return textureToEdit;
    }
    void addPointsForRaycast(List<Vector2> longestRedLine)
    {
        redPixelCoordinates.Add(new Vector2(longestRedLine[0].x, longestRedLine[0].y));

        List<Vector2> firstHalf = longestRedLine.GetRange(0, longestRedLine.Count / 2);
        List<Vector2> secondHalf = longestRedLine.GetRange(longestRedLine.Count / 2, (longestRedLine.Count-1)- longestRedLine.Count/2);

        int firstHalfSegmentLength = firstHalf.Count / 5;
        for (int i = 1; i < 5; i++)
        {
            Vector2 tmp = firstHalf[i * firstHalfSegmentLength];
            redPixelCoordinates.Add(new Vector2(tmp.x, tmp.y));
        }
        redPixelCoordinates.Add(new Vector2(longestRedLine[(longestRedLine.Count - 1) / 2].x, longestRedLine[(longestRedLine.Count - 1) / 2].y));

        int secondHalfSegmentLength = secondHalf.Count / 5;
        for (int i = 1; i < 5; i++)
        {
            Vector2 tmp = secondHalf[i * secondHalfSegmentLength];
            redPixelCoordinates.Add(new Vector2(tmp.x, tmp.y));
        }

        redPixelCoordinates.Add(new Vector2(longestRedLine[longestRedLine.Count - 1].x, longestRedLine[longestRedLine.Count - 1].y));
    }

    List<Vector2> SearchForRedPixels(int startX, int startY)
    {
        List<Vector2> currentLine = new List<Vector2>();

        bool hasFound = false;

        int foundX = startX, foundY = startY;
        int heightY = redPixels.GetLength(1);
        int withX = redPixels.GetLength(0);

        do
        {
            hasFound = false;
            redPixels[foundX, foundY] = false;
            currentLine.Add(new Vector2(foundX, foundY));

            for (int i = 1; i < 25 && foundX + i < withX; i++)
            {

                if (redPixels[foundX + i, foundY])
                {
                    foundX = foundX + i;
                    hasFound = true;
                }
            }

            for (int i = 1; i < 10 && !hasFound && foundY - i > 0; i++)
            {
                hasFound = redPixels[foundX, foundY - i];

                if (hasFound)
                {
                    foundY = foundY - i;
                }
            }

            for (int i = 1; i < 10 && !hasFound && foundY + i < heightY; i++)
            {
                hasFound = redPixels[foundX, foundY + i];

                if (hasFound)
                {
                    foundY = foundY + i;
                }
            }
        } while (hasFound);

        return currentLine;
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



    int moveToCounter = 1;
    int moveFromCounter;
    bool isAtEnd;
    // Update is called once per frame
    void Update()
    {
        if (shouldMove)
        {
            if (moveToCounter < cablePositinos.Count)
            {
                instantiatedObject.transform.Translate((cablePositinos[moveToCounter] - cablePositinos[moveFromCounter]) * 0.0075f);
                if (Vector3.Distance(instantiatedObject.transform.position, cablePositinos[moveToCounter]) < 0.00275f)
                {
                    instantiatedObject.transform.position = cablePositinos[moveToCounter];
                    Debug.Log("moveToCounter: " + moveToCounter + " moveFrom Counter: " + moveFromCounter);
                    moveToCounter++;
                    moveFromCounter++;
                    if(Vector3.Distance(instantiatedObject.transform.position, cablePositinos[cablePositinos.Count - 1]) < 0.00275f)
                    {
                        isAtEnd = true;
                    }
                }
            } else
            {
                shouldMove = false;
            }
        }
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
                            cablePositinos.Add(new Vector3(hits[i].pose.position.x, hits[i].pose.position.y + 0.05f, hits[i].pose.position.z));
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
    void DebugCameraReselution()
    {
        Debug.Log("Start Script");

        // Check if WebCamTexture is available
        if (WebCamTexture.devices.Length > 0)
        {
            WebCamDevice frontCamera = WebCamTexture.devices[0]; // Assuming the first camera is the desired one
            WebCamTexture webcamTexture = new WebCamTexture(frontCamera.name);
            webcamTexture.Play();

            // Check if the webcam texture is ready and has valid dimensions
            if (webcamTexture.width > 0 && webcamTexture.height > 0)
            {
                Debug.Log("Selected Resolution: " + webcamTexture.width + "x" + webcamTexture.height);
                // Do something with the resolution, e.g., set up a texture or camera
            }
            else
            {
                Debug.LogError("Failed to get a valid camera resolution.");
            }
        }
        else
        {
            Debug.LogError("No camera devices found.");
        }
    }
}