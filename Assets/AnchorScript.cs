using UnityEngine;

public class AnchorScript : MonoBehaviour//, IMixedRealityGestureHandler<Vector3>
{
    /*public GameObject placingGameObject;
    public GameObject anchorPrefab;
    public ActualContent actualContent;

    private void OnEnable()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityGestureHandler<Vector3>>(this);
    }

    private void OnDisable()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityGestureHandler<Vector3>>(this);
    }

    public void OnGestureStarted(InputEventData eventData)
    {
        if (eventData.InputSource.SourceType == InputSourceType.Hand &&
            eventData.InputSource.Pointers[0].IsInteractionEnabled)
        {
            if (eventData.InputSource.Pointers[0].Controller.ControllerHandedness == Handedness.Right)
            {
                PlaceObject(eventData.InputData);
            }
        }
    }

    public void OnGestureUpdated(InputEventData eventData)
    {
        // You can add behavior for gesture updates here if needed.
    }

    public void OnGestureCompleted(InputEventData eventData)
    {
        // You can add behavior for gesture completion here if needed.
    }

    public void OnGestureCanceled(InputEventData eventData)
    {
        // You can add behavior for gesture cancellation here if needed.
    }

    private void PlaceObject(Vector3 pinchPosition)
    {
        // Create an anchor at the pinch position.
        GameObject anchor = Instantiate(anchorPrefab, pinchPosition, Quaternion.identity);

        // Attach the actual content to the anchor.
        actualContent.AttachToAnchor(anchor);

        // Position the Placing Game Object at the anchor.
        placingGameObject.transform.position = anchor.transform.position;
    }*/
}
