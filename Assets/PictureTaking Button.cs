using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.WebCam;

public class PictureTakingButton : MonoBehaviour
{

    // Start is called before the first frame update
    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;

    // Use this for initialization
    public void Start()
    {
        Debug.Log("Start");
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Create a PhotoCapture object
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            if (captureObject != null)
            {
                photoCaptureObject = captureObject;
                CameraParameters cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 0.0f;
                cameraParameters.cameraResolutionWidth = cameraResolution.width;
                cameraParameters.cameraResolutionHeight = cameraResolution.height;
                cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

                // Activate the camera
                photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                    Debug.Log("Camera Activated");
                photoCaptureObject.TakePhotoAsync(onCapturedPhotoToMemory);
                    Debug.Log("Picture taken");
                    // Deactivate the camera
                photoCaptureObject.StopPhotoModeAsync(onStoppedPhotoMode);
                    Debug.Log("Camera deactivated");
                });
            }
            else
            {
                Debug.LogError("Capture object is null. Check your camera setup.");
            }
        });
    }

    void onCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        // Render the texture can to the Gemobject 
        Debug.Log("Render Texture to Gameobject");
        if (targetTexture != null)
        {
            targetTexture = editTexture(targetTexture);
            Renderer rend = GetComponent<Renderer>();
            rend.material.mainTexture = targetTexture;
        }
        else
        {
            Debug.LogError("New texture is not assigned!");
        }
    }

    void onStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown the photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    Texture2D editTexture(Texture2D textureToEdit)
    {
        // Get the pixel data from the texture
        Color[] pixels = textureToEdit.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            // Check if the pixel is predominantly red (you can adjust the threshold)
            if (pixels[i].r > 0.8f && pixels[i].g < 0.5f && pixels[i].b < 0.5f)
            {
                pixels[i] = Color.red;
            }
            else
            {
                pixels[i] = Color.black; // Set non-red pixels to black
            }
        }

        // Apply the edited pixels back to the texture
        textureToEdit.SetPixels(pixels);
        textureToEdit.Apply();
        return textureToEdit;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
