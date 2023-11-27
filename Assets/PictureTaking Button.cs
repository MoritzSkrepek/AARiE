
using MixedReality.Toolkit.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PictureTakingButton : MonoBehaviour
{

    // Start is called before the first frame update
    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;
    Resolution cameraResolution;
    public float redComponentThreshold = 0.7f;
    public ARPointCloudManager arPointCloudManager;
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
    }
    public void takingPicture()
    {
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

                photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                    Debug.Log("Camera Activated");
                    photoCaptureObject.TakePhotoAsync(onCapturedPhotoToMemory);
                });
            }
        });
    }

    void onCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        // Edit the texture
        Texture2D editedTexture = editTexture(targetTexture);

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
        // Calculate average Y coordinate for each unique X coordinate and add to redPixelCoordinates
        foreach (var kvp in yCoordinatesByX)
        {
            int x = kvp.Key;
            List<int> yCoordinates = kvp.Value;

            // Calculate average Y coordinate
            int averageY = Mathf.RoundToInt((float)yCoordinates.Average());

            // Add to redPixelCoordinates
            redPixelCoordinates.Add(new Vector2(x, averageY));
        }

        // Apply the edited pixels back to the texture
        textureToEdit.SetPixels(pixels);
        textureToEdit.Apply();

        Debug.Log("Texture edited");
        return textureToEdit;
    }


    // Update is called once per frame
    void Update()
    {
    }


    bool PositionVirtualObject(Vector2 redPixel, Texture2D image)
    {
        Vector2 screenCoordinates = new Vector2(
            redPixel.x / image.width * Screen.width,
            (image.height - redPixel.y) / image.height * Screen.height
        );
        Vector3 ndcCenter = new Vector3(
            (2.0f * screenCoordinates.x / Screen.width) - 1.0f,
            (2.0f * screenCoordinates.y / Screen.height) - 1.0f,
         1.0f
            );

        virtualObject.transform.position = ndcCenter;

        float scaleX = 1;
        float scaleY = 1;
        virtualObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        Debug.Log("NDC Center: " + ndcCenter);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        // Use Camera.main.transform.forward as the ray direction
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Debug.Log("Ray: " + ray);
        Debug.Log("raycastManager" + raycastManager);
        if ( raycastManager.Raycast(ray, hits) )
        {
            Debug.Log("HOLY SHIT ES GEHT, ES HITTET WAS" + hits[0]);
        }
        Debug.Log("hits" + hits[0]);
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

    Vector3 FindNearestFeaturePoint(Vector3 targetPoint, List<Vector3> featurePoints)
    {
        float minDistance = float.MaxValue;
        Vector3 nearestPoint = Vector3.zero;

        foreach (Vector3 point in featurePoints)
        {
            float distance = Vector3.Distance(targetPoint, point);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = point;
            }
        }

        return nearestPoint;
    }
}
