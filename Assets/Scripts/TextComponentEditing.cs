using TMPro;
using UnityEngine;

public class TextComponentEditing : MonoBehaviour
{
    public float scaleFactor = 1f;
    public GameObject textObject;

    public TextComponentEditing(GameObject textObject)
    {
        this.textObject = textObject;
    }

    public void EditText(string username, string message)
    {
        if (textObject != null && username != null && message != null)
        {
            TextMeshPro textMeshPro = textObject.GetComponent<TextMeshPro>();
            textMeshPro.text = "Absender: " + username + "\nNachricht: " + message;
            if (textMeshPro == null)
            {
                Debug.LogError("No TextMeshPro component found or assigned!");
                return;
            }
            GameObject background = textMeshPro.GetComponent<GameObject>();

            float textLength = textMeshPro.preferredWidth;

            background.transform.localScale = new Vector3(textLength * scaleFactor, transform.localScale.y, transform.localScale.z);
        }
    }
}