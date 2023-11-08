using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;
using MixedReality.Toolkit.UX;
using Unity.VisualScripting;

public class ToggleSpatialMeshVisibility : MonoBehaviour
{
    public GameObject mrtkXRRig;
    public PressableButton button;

    private bool isArMeshVisible = true;
    private ARMeshManager arMeshManager;
    private TextMeshProUGUI buttonTMPro;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(button.gameObject.name);
        if (button != null)
        {
            Debug.Log("Button is not null!");
            button.OnClicked.AddListener(ToggleVisibility);
            UpdateButton();
        }

        arMeshManager = mrtkXRRig.GetComponentInChildren<ARMeshManager>();
        buttonTMPro = button.GetComponentInChildren<TextMeshProUGUI>();

        Debug.Log(mrtkXRRig.gameObject.name);
        //Debug.Log(arMeshManager.gameObject.name);
    }

    void ToggleVisibility()
    {
        Debug.Log("Toggle");
        isArMeshVisible = !isArMeshVisible;

        if (arMeshManager != null)
        {
            arMeshManager.enabled = isArMeshVisible;
        }

        UpdateButton();
    }

    void UpdateButton()
    {
        if (button != null && buttonTMPro != null)
        {
            string textValue = buttonTMPro.text;
            Debug.Log("Button Text (TextMeshPro): " + textValue);

            buttonTMPro.text = isArMeshVisible ? "Turn off Mesh" : "Turn on Mesh";
        }
    }
}
