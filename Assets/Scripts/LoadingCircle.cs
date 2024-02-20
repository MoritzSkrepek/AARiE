using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCircle : MonoBehaviour
{
    public GameObject border;
    private float runningTime = 0.0f;
    private float fillSpeed = 0.05f;
    private bool isRunning = false;

    private Image img;

    // Start is called before the first frame update
    void Start()
    {
        img = border.GetComponent<Image>();
        img.fillAmount = 0f;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartLoading()
    {
        img = border.GetComponent<Image>();
        isRunning = true;
        StartCoroutine(LoadingCoroutine());
    }
    public void StopLoading()
    {
        isRunning = false;
        StopCoroutine(LoadingCoroutine());
    }
    IEnumerator LoadingCoroutine()
    {
        while (true)
        {
            if (img.fillAmount >= 1f)
            {
                img.fillAmount = 0f;
            }
            else
            {
                img.fillAmount += fillSpeed;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
