using UnityEngine;

public class ToggleVisibility : MonoBehaviour
{
    public GameObject menuToToggle;
    private bool isVisible = false;

    private void Start()
    {
        // Ensure the menu starts in the desired initial state
        SetVisibility(isVisible);
    }

    public void OnButtonClick()
    {
        // Toggle the visibility of the menu
        isVisible = !isVisible;
        SetVisibility(isVisible);
    }

    private void SetVisibility(bool isVisible)
    {
        // Set the visibility of the menu
        menuToToggle.SetActive(isVisible);
    }
}
