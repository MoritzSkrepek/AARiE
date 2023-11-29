using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    public ARPointCloudManager arPointCloudManager;
    public ARPlaneManager arPlaneManager;
    List<Vector2> redPixelCoordinates = new List<Vector2>();

    public ARRaycastManager raycastManager;

    [SerializeField]
    private GameObject virtualObject;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Start");
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);


        //GameObject g1 = Instantiate(virtualObject, new Vector3(0, 0, 0), Quaternion.identity);
        //g1.GetComponent<Renderer>().material.SetColor("_Color", Color.);
    }
    public void takingPicture()
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
                PositionVirtualObject(redPixle, targetTexture);
            }
        }
        else
        {
            Debug.Log("No Red Pixel found");
        }
        takingNewPicture = false;
    }

    Texture2D editTexture(Texture2D textureToEdit)
    {
        // Get the pixel data from the texture
        Color[] pixels = textureToEdit.GetPixels();

        int textureWidth = textureToEdit.width;

        Dictionary<int, List<int>> yCoordinatesByX = new Dictionary<int, List<int>>();

        for (int y = 0; y < textureToEdit.height; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                int index = y * textureWidth + x;

                // Check if the pixel is predominantly red (you can adjust the threshold)
                if (pixels[index].r > redComponentThreshold && pixels[index].g < 0.5f && pixels[index].b < 0.5f)
                {
                    pixels[index] = Color.red;

                    // Add Y coordinate to the list for the current X coordinate
                    if (!yCoordinatesByX.ContainsKey(x))
                    {
                        yCoordinatesByX[x] = new List<int>();
                    }
                    yCoordinatesByX[x].Add(y);
                }
                else
                {
                    pixels[index] = Color.black; // Set non-red pixels to black
                }
            }
        }

        if (yCoordinatesByX.Count == 0)
        {
            return textureToEdit;
        }

        int totalX = 0;
        int totalRedPixels = 0;
        int averageY = 0;
        int totalY = 0;

        // Calculate total X coordinates and total number of red pixels
        foreach (var kvp in yCoordinatesByX)
        {
            int x = kvp.Key;
            List<int> yCoordinates = kvp.Value;

            // Calculate average Y coordinate
            averageY = Mathf.RoundToInt((float)yCoordinates.Average());

            totalX += x;
            totalY += averageY;
            totalRedPixels++;
        }

        // Calculate average X coordinate
        int averageX = Mathf.RoundToInt((float)totalX / totalRedPixels);
        averageY = Mathf.RoundToInt((float)totalY / totalRedPixels);
        // Add the average X, average Y to redPixelCoordinates
        redPixelCoordinates.Add(new Vector2(averageX, averageY));

        // Apply the edited pixels back to the texture
        textureToEdit.SetPixels(pixels);
        textureToEdit.Apply();

        Debug.Log("Texture edited");
        return textureToEdit;
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
        // Add the average X, average Y to redPixelCoordinates
        redPixelCoordinates.Add(new Vector2(averageX, averageY));

        // Apply the edited pixels back to the texture
        textureToEdit.SetPixels(pixels);
        textureToEdit.Apply();

        Debug.Log("Texture edited");
        return textureToEdit;
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


        Debug.Log("screencords: " + screenCoordinates.x + " " + screenCoordinates.y);
        Debug.Log("redpixel: " + redPixel.x + " " + redPixel.y);
        Debug.Log("image size: " + image.width + " " + image.height);

        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (raycastManager.Raycast(screenCoordinates, hits, TrackableType.Planes))
        {
            Debug.Log("COUNT: " + hits.Count);

            for (int i = 0; i < hits.Count; i++)
            {
                if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
                {
                    debugRaycast(hits[i], Color.blue);
                    Debug.Log("first hit (blue) ");
                }
            }
        }

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
        }



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

        //if (raycastManager.Raycast(new Vector2(1, 1), hits, TrackableType.Planes))
        //{
        //    Debug.Log("COUNT: " + hits.Count);

        //    for (int i = 0; i < hits.Count; i++)
        //    {
        //        if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
        //        {
        //            drawLine(hits[i], new Color(153, 102, 51));
        //            Debug.Log("first hit (153, 102, 51) ");
        //            GameObject g2 = Instantiate(virtualObject, hits[i].pose.position, Quaternion.identity);
        //            g2.GetComponent<Renderer>().material.SetColor("_Color", new Color(153, 102, 51));
        //            break;
        //        }
        //    }
        //}

        //Debug.Log("cords: " + Camera.main.ViewportPointToRay(new Vector3(screenCoordinates.x, screenCoordinates.y)) 
        //    + " " + Camera.main.ScreenToViewportPoint(new Vector3(screenCoordinates.x, screenCoordinates.y))
        //    + "" + Camera.main.WorldToScreenPoint(new Vector3(screenCoordinates.x, screenCoordinates.y)));



        //Ray ray2 = new Ray(Camera.main.transform.position, ndcCenter);
        //if ( raycastManager.Raycast(ray2, hits, TrackableType.Planes) )
        //{
        //    Debug.Log("COUNT: " + hits.Count);

        //    for (int i = 0; i < hits.Count; i++)
        //    {
        //        if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
        //        {
        //            hitPlanes.Append(hits[i]);
        //            Debug.Log("HOLY SHIT ES GEHT, ES HITTET WAS: " + hits[i].pose.position);
        //            Debug.Log("Hits type: " + hits[i].hitType);
        //            //GameObject g1 = Instantiate(virtualObject, hits[i].pose.position, Quaternion.identity);

        //            if (bFirst)
        //            {
        //                drawLine(hits[i], Color.green);
        //                Debug.Log("first hit (green) ");
        //                GameObject g2 = Instantiate(virtualObject, hits[i].pose.position, Quaternion.identity);
        //                g2.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        //                bFirst = false;
        //            }
        //        }
        //    }
        //}


        //ARPlane plane = FindClosestPlane(hits);
        //if (plane != null)
        //{
        //    Debug.Log("find colsest (green): " + plane.center);

        //    GameObject g1 = Instantiate(virtualObject, plane.center, Quaternion.identity);
        //    g1.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        //}
        //else
        //{
        //    Debug.Log("plane = null");
        //}

        //    ARPlane plane2 = FindClosestPlane(hitPlanes);
        //    if (plane2 != null)
        //    {
        //        Debug.Log("find colsest (blue): " + plane2.center);

        //        GameObject g1 = Instantiate(virtualObject, plane2.center, Quaternion.identity);
        //        g1.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        //    }
        //    else
        //    {
        //        Debug.Log("plane2 = null");
        //    }

        // Vector3 v = FindNearestFeaturePoint(hits);

        //GameObject g3 = Instantiate(virtualObject, v, Quaternion.identity);
        //g3.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        //drawLine(v,Color.red);

        //Vector3 v1 = FindNearestFeaturePoint(hitPlanes);

        //GameObject g4 = Instantiate(virtualObject, v1, Quaternion.identity);
        //g4.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        //drawLine(v1,Color.blue);

        ////if (raycastManager.Raycast(ndcCenter, hits, TrackableType.Planes))
        //{
        //    Debug.Log("COUNT: " + hits.Count);

        //    for (int i = 0; i < hits.Count; i++)
        //    {
        //        if (hits[i].hitType != TrackableType.PlaneWithinInfinity)
        //        {
        //            hitPlanes.Append(hits[i]);
        //            Debug.Log("HOLY SHIT ES GEHT, ES HITTET WAS: " + hits[i].pose.position);
        //            Debug.Log("Hits type: " + hits[i].hitType);
        //            //GameObject g1 = Instantiate(virtualObject, hits[i].pose.position, Quaternion.identity);

        //            if (bFirst)
        //            {
        //                drawLine(hits[i], Color.grey);
        //                Debug.Log("first hit (grey) ");
        //                GameObject g2 = Instantiate(virtualObject, hits[i].pose.position, Quaternion.identity);
        //                g2.GetComponent<Renderer>().material.SetColor("_Color", Color.grey);
        //                bFirst = false;
        //            }
        //        }
        //    }
        //}
        //if (raycastManager.Raycast(new Vector2(0, 0), hits))
        //{
        //    Debug.Log("HOLY SHIT ES GEHT, ES HITTET WAS 0,0" + hits);
        //    //GameObject g3 =  Instantiate(virtualObject, hits[0].pose.position, Quaternion.identity);

        //}
        return true;
        // Use ARPointCloudManager to find ARPointCloud
        //ARPointCloud arPointCloud = null;
        //Debug.Log(" count" + arPointCloudManager.trackables.count);
        //foreach (var pointCloud in arPointCloudManager.trackables)
        //{
        //    Debug.Log("pointcloud:" + pointCloud);
        //    arPointCloud = pointCloud as ARPointCloud;
        //    if (arPointCloud != null)
        //        break;
        //}

        //if (arPointCloud != null)
        //{
            // Create a managed list to store feature points
            //List<Vector3> featurePoints = new List<Vector3>();

            //// Copy feature points from NativeSlice to managed list
            ////foreach (var position in arPointCloud.positions)
            ////{
            ////    featurePoints.Add(position);
            ////    Debug.Log("featurePoints position added: " + position);
            ////}

            //// Map NDC to world space position using the ARCamera
            //Ray ray = Camera.main.ViewportPointToRay(new Vector3(ndcCenter.x, ndcCenter.y, Camera.main.transform.position.z));
            //Debug.Log("Ray: " + ray);

            //// Use ARRaycastManager for raycasting
            //List<ARRaycastHit> hits = new List<ARRaycastHit>();
            //if (ARRaycastManager.Raycast(ray, hits))
            //{
            //    Debug.Log("HOLY SHIT ES GEHT, ES HITTET WAS");
            //    // Find the nearest feature point to the hit point
            //    Vector3 hitPoint = hits[0].pose.position;
            //    //Vector3 nearestFeaturePoint = FindNearestFeaturePoint(hitPoint, featurePoints);

            //    // Set the position of the virtual object to the nearest feature point in AR world space
            //    virtualObject.transform.position = hitPoint;

            //    // Set the scale of the virtual object based on the detected red region
            //    float scaleX = 1;
            //    float scaleY = 1;
            //    virtualObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            //    return true;
            //}
            //else
            //{
            //    // Handle when the raycast does not hit any AR trackable
            //    Debug.LogWarning("Raycast did not hit any AR trackable.");
            //    return false;
            //}
        //}
        //else
        //{
        //    // Handle when ARPointCloud is not available
        //    Debug.LogWarning("ARPointCloud is not available.");
        //    return false;
        //}
    }
    private List<GameObject> instantiatedObjects = new List<GameObject>();

    void debugRaycast(ARRaycastHit hit, Color color)
    {
        // Create a new game object for each line rendering operation
        //GameObject lineObject = new GameObject("LineRendererObject_" + instantiatedObjects.Count());
        //LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        //// Configure the LineRenderer component
        //lineRenderer.startColor = color;
        //lineRenderer.endColor = color;
        //lineRenderer.startWidth = 0.01f;
        //lineRenderer.endWidth = 0.01f;
        //lineRenderer.SetPosition(0, Camera.main.transform.position);
        //lineRenderer.SetPosition(1, hit.pose.position);

        Debug.Log("Ray hit position: " + hit.pose.position);

        GameObject instantiatedObject = Instantiate(virtualObject, hit.pose.position, Quaternion.identity);
        instantiatedObject.GetComponent<Renderer>().material.SetColor("_Color", color);

        // Keep track of the instantiated objects
        instantiatedObjects.Add(instantiatedObject);
    }
    void debugRaycast(Vector3 hit, Color color)
    {
        Debug.Log("Ray hit position: " + hit);

        GameObject instantiatedObject = Instantiate(virtualObject, hit, Quaternion.identity);
        instantiatedObject.GetComponent<Renderer>().material.SetColor("_Color", color);

        // Keep track of the instantiated objects
        instantiatedObjects.Add(instantiatedObject);
    }

    Vector3 FindNearestFeaturePoint(List<ARRaycastHit> hits)
    {
        float minDistance = float.MaxValue;
        Vector3 nearestPoint = Vector3.zero;

        foreach (ARRaycastHit hit in hits)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, hit.pose.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = hit.pose.position;
            }
        }

        return nearestPoint;
    }
}
