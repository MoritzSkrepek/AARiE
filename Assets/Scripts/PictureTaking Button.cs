using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.GraphicsBuffer;
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
    private Transform camera;

    public ARRaycastManager raycastManager;

    [SerializeField]
    private GameObject virtualObject;

    public GameObject sendPackageButton;

    public GameObject scanningButton;

    public GameObject infoTextT;

    public GameObject infoTextS;

    void Start()
    {
        Debug.Log("Start Script");
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).FirstOrDefault();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        camera = Camera.main.transform;
    }
    
public void takingPicture()
    {
        if (!takingNewPicture)
        {
            redPixelCoordinates.Clear();

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
        Stopwatch timer6 = Stopwatch.StartNew();
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);
        // Edit the texture
        Texture2D editedTexture = editTextureV3(targetTexture);

        // Set the edited texture to the material of the Renderer
        Renderer rend = GetComponent<Renderer>();
        rend.material.mainTexture = editedTexture;

        // Deactivate the camera
        photoCaptureObject.StopPhotoModeAsync(onStoppedPhotoMode);

        timer6.Stop();
        Debug.Log("Time for everything togheder: " + timer6.ElapsedMilliseconds);

    }

    void onStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown the photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;

        if (redPixelCoordinates.Count != 0)
        {

            Stopwatch timer = Stopwatch.StartNew();
            foreach (Vector2 redPixle in redPixelCoordinates)
            {
                if(PositionVirtualObject(redPixle, targetTexture))
                {
                    sendPackageButton.SetActive(true);
                    scanningButton.SetActive(false);
                }
            }

            timer.Stop();
            Debug.Log("Time to for Raycast " + timer.ElapsedMilliseconds);
        }
        takingNewPicture = false;
    }

    public void ShowInformation()
    {
        if (showInformation == 0)
        {
            infoTextT.SetActive(true);
            infoTextT.transform.position = cablePositinos[(cablePositinos.Count - 1) / 2] + new Vector3(0, 0.10f, 0.1f);
            showInformation = 1;
        }
        else if (showInformation == 1)
        {
            infoTextT.SetActive(false);
            infoTextS.SetActive(true);
            infoTextS.transform.position = cablePositinos[(cablePositinos.Count-1)/2] + new Vector3(0, 0.10f, 0.1f);
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

    public NativeArray<float> result;
    private List<Vector2> redPixels = new List<Vector2>();
    Color[] pixels;
    Texture2D editTextureV3(Texture2D textureToEdit)
    {
        Stopwatch timer2 = Stopwatch.StartNew();
        Debug.Log("start search for red Pixel");
        pixels = textureToEdit.GetPixels();
        for (int x = 0; x < targetTexture.width; x ++)
        {
            for (int y = 0; y < targetTexture.height; y++)
            {
                int index = y * targetTexture.width + x;
                if (pixels[index].r > 0.7f && pixels[index].g < 0.5f && pixels[index].b < 0.5f)
                {
                    pixels[index] = Color.red;
                    redPixels.Add(new Vector2(x, y));
                } else
                {
                    pixels[index] = Color.black;
                }
            }
        }
        timer2.Stop();
        Debug.Log("Time finding redPixles: " + timer2.ElapsedMilliseconds);
        Stopwatch timer = Stopwatch.StartNew();
        Debug.Log("Red Pixle Count " + redPixels.Count);
        List<Vector2> longestRedLine = new List<Vector2>();
        if (redPixels.Count > 0)
            { 
            for (int targetY = 0; targetY < cameraResolution.height; targetY++)
            {
                if (redPixels.Any(v => v.y == targetY))
                {
                    List<Vector2> currentRedLine = new List<Vector2>();
                    Vector2 redPixelWithTargetY = redPixels.FirstOrDefault(v => v.y == targetY);
                    currentRedLine = SearchForRedPixels(redPixelWithTargetY, currentRedLine);
                    Debug.Log("Line done" + currentRedLine.Count());
                    if (currentRedLine.Count > longestRedLine.Count)
                    {
                        longestRedLine = currentRedLine;
                    }
                }
            }
                if (longestRedLine.Count > 0)
                {
                redPixelCoordinates.Add(new Vector2(longestRedLine[0].x, longestRedLine[0].y));
                Vector2 maxyCoordinate = redPixels.OrderByDescending(v => v.y).First();
                redPixelCoordinates.Add(new Vector2(maxyCoordinate.x, maxyCoordinate.y));
                redPixelCoordinates.Add(new Vector2(longestRedLine[(longestRedLine.Count - 1)/2].x, longestRedLine[(longestRedLine.Count - 1) / 2].y));
                Vector2 minyCoordinate = redPixels.OrderBy(v => v.y).First();
                redPixelCoordinates.Add(new Vector2(minyCoordinate.x, minyCoordinate.y));
                redPixelCoordinates.Add(new Vector2(longestRedLine[longestRedLine.Count - 1].x, longestRedLine[longestRedLine.Count - 1].y));
                }
                else
                {
                    Debug.Log("No red pixel found");
                }
            } else
            {
                Debug.Log("No red pixel found");
            }

        Debug.Log("count longest" + longestRedLine.Count);
        timer.Stop();
        Debug.Log("Time editing Texture: " + timer.ElapsedMilliseconds);
        for (int i = 0; i < longestRedLine.Count; i++)
        {
            pixels[(int)(longestRedLine[i].y * targetTexture.width + longestRedLine[i].x)] = Color.magenta;
        }
        textureToEdit.SetPixels(pixels);
        textureToEdit.Apply();
        return textureToEdit;
    }

    List<Vector2> SearchForRedPixels(Vector2 startPoint, List<Vector2> currentLine)
    {
        bool isChecked = false;
        redPixels.Remove(startPoint);
        currentLine.Add(startPoint);

        Vector2 foundPixel = new Vector2();

        for (int i = 1; i < 50 && !isChecked; i++) // Job System
        {
            foundPixel = new Vector2(startPoint.x + i, startPoint.y);
            isChecked = redPixels.Contains(foundPixel) && !currentLine.Contains(foundPixel);
        }

        for (int i = 1; i < 50 && !isChecked; i++)
        {
            foundPixel = new Vector2(startPoint.x, startPoint.y - i);
            isChecked = redPixels.Contains(foundPixel) && !currentLine.Contains(foundPixel);
        }

        for (int i = 1; i < 50 && !isChecked; i++)
        {
            foundPixel = new Vector2(startPoint.x, startPoint.y +i);
            isChecked = redPixels.Contains(foundPixel) && !currentLine.Contains(foundPixel);
        }

        if ( isChecked )
        { 
            SearchForRedPixels(foundPixel, currentLine);
        }

        
        return currentLine;
    }


    /*    List<Vector2> SearchForRedPixels(Vector2 startPoint, List<Vector2> currentLine)
    {

        redPixels.Remove(startPoint);
        currentLine.Add(startPoint);

        Vector2 pixelNeighboring = new Vector2(startPoint.x + 1, startPoint.y);
        if (redPixels.Contains(pixelNeighboring) && !currentLine.Contains(pixelNeighboring))
        {
            SearchForRedPixels(pixelNeighboring, currentLine);
        }
        else
        {
            
            Vector2 pixelAbove = new Vector2(startPoint.x, startPoint.y - 1);
            if (redPixels.Contains(pixelAbove) && !currentLine.Contains(pixelAbove))
            {
                SearchForRedPixels(pixelAbove, currentLine);
            }
            Vector2 pixelBelow = new Vector2(startPoint.x, startPoint.y + 1);
            if (redPixels.Contains(pixelBelow) && !currentLine.Contains(pixelBelow))
            {
                SearchForRedPixels(pixelBelow, currentLine);
            }
        }
        return currentLine;
    }*/


    private void ProcessTexture()
    {
        Debug.Log("Start processing texture");


        int threadCount = Mathf.Min(SystemInfo.processorCount, targetTexture.width);
        ManualResetEvent[] doneEvents = new ManualResetEvent[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            doneEvents[i] = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((state) =>
            {
                CheckForRedPixels(i, threadCount);
                doneEvents[i].Set();
            });
        }

        // Wait for all threads to finish
        WaitHandle.WaitAll(doneEvents);

        Debug.Log("Texture processing complete");
    }


    private void CheckForRedPixels(int startIndex, int step)
    {
        Debug.Log("c1"); // Here is the problem
        for (int x = startIndex; x < targetTexture.width; x += step)
        {
            for (int y = 0; y < targetTexture.height; y++)
            {
                int index = y * targetTexture.width + x;
                if (pixels[index].r > 0.55f && pixels[index].g < 0.5f && pixels[index].b < 0.5f)
                {
                    lock (redPixels)
                    {
                        redPixels.Add(new Vector2(x, y));
                    }
                }
            }
        }
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
            instantiatedObject.transform.Translate((cablePositinos[cablePositinos.Count - 1] - cablePositinos[0]) * 0.02f);
            if (Vector3.Distance(instantiatedObject.transform.position, cablePositinos[2]) < 0.01f)
            {
                instantiatedObject.transform.position = cablePositinos[2];


                if (Vector3.Distance(instantiatedObject.transform.position, cablePositinos[cablePositinos.Count - 1]) < 0.01f)
                {
                    shouldMove = !shouldMove;
                }
            }
        }
        if (!shouldMove && isObjectInstantiated)
        {
            instantiatedObject.transform.LookAt(camera);
        }
    }

    /*
     * 
     * 
        if (shouldMove) // Add Movement to every position
        {
            shouldMove = false;
            movePackage();
        }
     * 
     * void movePackage()
    {
        int timesMoved = 0;
        Debug.Log("1 timesMoved: " + timesMoved + " cablePos Count: " + (cablePositinos.Count - 1));
        if (timesMoved < cablePositinos.Count - 2)
        {
            Debug.Log("2");
            moveToPostition(cablePositinos[timesMoved], cablePositinos[timesMoved + 1]);
            Debug.Log("3");
            timesMoved++;
        }
    }
    void moveToPostition(Vector3 startpoint, Vector3 endPoint)
    {
        instantiatedObject.transform.Translate((endPoint - startpoint) * 0.02f);
        bool isOnMove = true;
            while(isOnMove) {
                if (Vector3.Distance(instantiatedObject.transform.position, endPoint) < 0.01f)
                {
                    isOnMove = false;
                    instantiatedObject.transform.position = endPoint;
                }
        }
            
    }
    */




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