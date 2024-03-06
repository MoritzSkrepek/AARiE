



using TMPro;
using UnityEngine;

public class EditTextOfInformationObject : MonoBehaviour
{
    public GameObject parentGameObject;
    public GameObject background;
    TextMeshPro textMeshPro;
    public float scaleFactor = 1f;

    public EditTextOfInformationObject(GameObject parentGameObject)
    {
        this.parentGameObject = parentGameObject;
    }
    public void GetTextMeshProFromChild()
    {
        if (parentGameObject != null)
        {
            GameObject childGameObject = parentGameObject.transform.Find("InformationTextS").gameObject;

            if (childGameObject != null)
            {
                TextMeshPro tmp = childGameObject.GetComponent<TextMeshPro>();

                if (tmp != null)
                {
                    textMeshPro = tmp;
                }
                else
                {
                    Debug.LogError("TextMeshPro component not found on the child GameObject.");
                }
            }
            else
            {
                Debug.LogError("Child GameObject with name 'InformationTextS' not found.");
            }
        }
        else
        {
            Debug.LogError("Parent GameObject not assigned.");
        }
    }
    public void EditText(string username, string message)
    {
        if (textMeshPro != null && username != null && message != null)
        {
            textMeshPro.text = "Absender: " + username + "\n\nNachricht: " + message;
        }
    }
}