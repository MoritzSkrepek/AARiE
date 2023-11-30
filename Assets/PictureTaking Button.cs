using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Windows.WebCam;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PictureTakingButton : MonoBehaviour
{

    // Start is called before the first frame update
    bool takingNewPicture = false;
    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
    Resolution cameraResolution;
    public float redComponentThreshold = 0.7f;
    public ARPlaneManager arPlaneManager;
    private List<Vector3> cablePositinos = new List<Vector3>();
    List<Vector2> redPixelCoordinates = new List<Vector2>();
    GameObject instantiatedObject;
    bool isLeft = true, shouldMove = false;
    bool showInformation = false;

    public ARRaycastManager raycastManager;

    [SerializeField]
    private GameObject virtualObject;

    [SerializeField]
    private GameObject checkPointObject;

    public GameObject sendPackage1;
    public GameObject scanningButton;

    // Used this for initialization
    void Start()
    {
        Debug.Log("Start");
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
    }
    void takingPicture()
    {
        if (!takingNewPicture)
        {
            redPixelCoordinates.Clear();
            takingNewPicture = true;
            Debug.Log("Start taking Picture");

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
                            Debug.Log("Camera Activated");
                            photoCaptureObject.TakePhotoAsync(onCapturedPhotoToMemory);
                        });

                        //GameObject g2 = Instantiate(virtualObject, Camera.main.transform.position, Quaternion.identity);
                        //g2.GetComponent<Renderer>().material.SetColor("_Color", Color.black);

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
                    Debug.Log("Activate buttons");
                    sendPackage1.SetActive(true);
                    scanningButton.SetActive(false);
                }
            }
        }
        else
        {
            Debug.Log("No Red Pixel found");
        }
        takingNewPicture = false;
    }
    public void sendPackage()
    {
        //if(isLeft)
        //{
        //    moveEnvelope(cablePositinos[0], cablePositinos[2]);
        //} else
        //{
        //    moveEnvelope(cablePositinos[2], cablePositinos[0]);
        //}

        if (instantiatedObject == null)
        {
            instantiatedObject = Instantiate(virtualObject, cablePositinos[0], Quaternion.identity);
        }

        isLeft = !isLeft;
        shouldMove = true;
    }


    void moveEnvelope(Vector3 from,Vector3 to)
    {
        if (instantiatedObject == null)
        {
            instantiatedObject = Instantiate(virtualObject, from, Quaternion.identity);
        }
        Vector3 pass = cablePositinos[1];
        while (true)
        {
            if (Vector3.Distance(from, pass) < 0.1f)
            {
                while (true)
                {
                    if (Vector3.Distance(pass, to) < 0.1f)
                    {
                        break;
                    }
                    else
                    {
                        wait();
                        Debug.Log("Pass1: " + pass);
                        pass = Vector3.MoveTowards(pass, to, 0.05f);
                        Debug.Log("Pass2: " + pass);
                        instantiatedObject.transform.position = pass;
                    }
                }
                break;
            }
            else
            {
                wait();
                from = Vector3.MoveTowards(from, pass, 0.05f);
                instantiatedObject.transform.position = from;
            }
        }

    }

    private IEnumerator wait()
    {
        yield return new WaitForSeconds(10);
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
        //redPixelCoordinates.Add(new Vector2(xCoordinates.Min(), averageY));
        // Add the average X, average Y to redPixelCoordinates
        redPixelCoordinates.Add(getSideCoordinate(xCoordinates,true, averageY));
        redPixelCoordinates.Add(new Vector2(averageX, averageY));
        redPixelCoordinates.Add(getSideCoordinate(xCoordinates, false, averageY));
        //redPixelCoordinates.Add(new Vector2(xCoordinates.Max(),averageY));

        // Apply the edited pixels back to the texture
        textureToEdit.SetPixels(pixels);
        textureToEdit.Apply();

        Debug.Log("Texture edited");
        return textureToEdit;
    }

    Vector2 getSideCoordinate(List<int> list,bool min, int averageY)
    {
        list.Sort();
        int tmp = 0;
        if (min)
        {
            for (int i = 0; i < list.Count/10; i++) 
            {
                tmp += list[i];
            }
        } else
        {
            int cal = list.Count - (list.Count / 10);
            for (int i = 0; i < list.Count / 10; i++)
            {
                tmp += list[cal + i];
            }
        }
        return new Vector2(tmp / (list.Count / 10), averageY);
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
            if (showInformation)
            {
                showInformation = false;
                //informationText.SetActive(false);
            }
            Debug.Log("move");
            if (isLeft)
            {
                Debug.Log("move left " + instantiatedObject.transform.position + (cablePositinos[2] - cablePositinos[0]));
                instantiatedObject.transform.Translate((cablePositinos[2] - cablePositinos[0]) * 0.01f);

                Debug.Log("move left after " + instantiatedObject.transform.position);
                if (Vector3.Distance(instantiatedObject.transform.position, cablePositinos[2]) < 0.01f)
                {
                    shouldMove = false;
                }
            }
            else
            {
                Debug.Log("move right + " + instantiatedObject.transform.position);
                instantiatedObject.transform.Translate((cablePositinos[0] - cablePositinos[2]) * 0.01f);

                Debug.Log("move right after " + instantiatedObject.transform.position);
                if (Vector3.Distance(instantiatedObject.transform.position, cablePositinos[0]) < 0.01f)
                {
                    shouldMove = false;
                }
            }
        } else
        {
            if (showInformation)
            {
                //informationText.SetActive(true);
            }
        }
    }

    public void showInfo()
    {
        showInformation = !showInformation;
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

        Vector2 middle = screenCoordinates - new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);

        //(Debug) For comparison of the screen coordinates and the red pixel coordinates    
        //
        //Debug.Log("screencords: " + screenCoordinates.x + " " + screenCoordinates.y);
        //Debug.Log("redpixel: " + redPixel.x + " " + redPixel.y);
        //Debug.Log("image size: " + image.width + " " + image.height);

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        
        if (raycastManager.Raycast(screenCoordinates, hits, TrackableType.Planes))
        { 

            Debug.Log("COUNT: " + hits.Count);

            for (int i = 0; i < hits.Count; i++)
                {
                    Debug.Log("Debug 1 Hit numero " + i);
                    if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
                    {
                        if (hits[i] != null)
                        {
                            cablePositinos.Add(new Vector3(hits[i].pose.position.x, hits[i].pose.position.y + 0.25f, hits[i].pose.position.z));
                            debugRaycast(hits[i], Color.red);
                        break;
                        } else
                        {
                            Debug.Log("Hit " + hits[i] + " is null");
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

        /*
        if (raycastManager.Raycast(new Vector2(Camera.main.pixelWidth / 2 + middle.x, Camera.main.pixelHeight / 2 +  middle.y), hits, TrackableType.Planes))
        {
            Debug.Log("COUNT: " + hits.Count);

            for (int i = 0; i < hits.Count; i++)
            {
                if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
                {
                    debugRaycast(hits[i], Color.red);
                    Debug.Log("first hit (red) ");
                    break;
                }
            }
        }*/



        // check 
        //float num = 500f;
        //if (raycastManager.Raycast(new Vector2(0, 0), hits, TrackableType.Planes))
        //{
        //    Debug.Log("COUNT: " + hits.Count);

        //    for (int i = 0; i < hits.Count; i++)
        //    {
        //        if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
        //        {
        //            drawLine(hits[i], Color.red);
        //            Debug.Log("first hit (red) ");
        //            break;
        //        }
        //    }
        //}

        //if (raycastManager.Raycast(new Vector2(num, num), hits, TrackableType.Planes))
        //{
        //    Debug.Log("COUNT: " + hits.Count);

        //    for (int i = 0; i < hits.Count; i++)
        //    {
        //        if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
        //        {
        //            drawLine(hits[i], Color.green);
        //            Debug.Log("first hit (green) ");
        //            break;
        //        }
        //    }
        //}

        //if (raycastManager.Raycast(new Vector2(num, -num), hits, TrackableType.Planes))
        //{
        //    Debug.Log("COUNT: " + hits.Count);

        //    for (int i = 0; i < hits.Count; i++)
        //    {
        //        if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
        //        {
        //            drawLine(hits[i], Color.blue);
        //            Debug.Log("first hit (blue) ");
        //            break;
        //        }
        //    }
        //}

        //if (raycastManager.Raycast(new Vector2(-num, num), hits, TrackableType.Planes))
        //{
        //    Debug.Log("COUNT: " + hits.Count);

        //    for (int i = 0; i < hits.Count; i++)
        //    {
        //        if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
        //        {
        //            drawLine(hits[i], Color.yellow);
        //            Debug.Log("first hit (yellow) ");
        //            break;
        //        }
        //    }
        //}


        //if (raycastManager.Raycast(new Vector2(-num, -num), hits, TrackableType.Planes))
        //{
        //    Debug.Log("COUNT: " + hits.Count);

        //    for (int i = 0; i < hits.Count; i++)
        //    {
        //        if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
        //        {
        //            drawLine(hits[i], Color.magenta);
        //            Debug.Log("first hit (magenta) ");
        //            break;
        //        }
        //    }
        //}
}
