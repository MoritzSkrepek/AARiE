using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCircle : MonoBehaviour
{
    public GameObject border;
    private float fillSpeed = 0.1f;
    private bool isRunnig = false;
    Stopwatch timer = new Stopwatch();

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
        isRunnig = true;
        timer.Start();
        img = border.GetComponent<Image>();
        StartCoroutine(LoadingCoroutine());
    }
    public void StopLoading()
    {
        isRunnig = false;
        timer.Stop();
        UnityEngine.Debug.Log("StopLoading timer: " + timer.ElapsedMilliseconds);
        StopCoroutine(LoadingCoroutine());
    }
    IEnumerator LoadingCoroutine()
    {
        while (isRunnig)
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
