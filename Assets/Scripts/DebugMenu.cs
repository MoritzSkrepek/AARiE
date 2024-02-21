using UnityEngine;

public class ToggleMenu : MonoBehaviour
{
    public GameObject menu; // Reference to your menu GameObject

    // Start is called before the first frame update
    void Start()
    {
        // Ensure the menu is initially hidden
        menu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Check for input, e.g., tapping on HoloLens 2
        if (Input.GetMouseButtonDown(0) && IsPointerOverButton())
        {
            ToggleMenuVisibility();
        }
    }

    void ToggleMenuVisibility()
    {
        // Toggle the menu's active state
        menu.SetActive(!menu.activeSelf);
    }

    bool IsPointerOverButton()
    {
        // Check if the user's gaze is over the button
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                return true;
            }
        }

        return false;
    }
}