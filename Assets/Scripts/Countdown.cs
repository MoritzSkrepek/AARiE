using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    public float totalTime = 120f;
    private float currentTime;
    private bool countdownStarted = false;

    public GameObject count;
    public GameObject border;

    private TextMeshProUGUI text;
    private UnityEngine.UI.Image img;

    // Start is called before the first frame update
    void Start()
    {
        StartCountdown();
    }

    // Update is called once per frame
    void Update()
    {
        if (countdownStarted)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                text.SetText(((int)currentTime).ToString());
                img.fillAmount = currentTime / totalTime;

                if (currentTime < 30 && currentTime > 10)
                {
                    img.color = Color.yellow;
                    text.color = Color.yellow;
                }

                if (currentTime < 10 && (int)currentTime % 2 == 0)
                {
                    img.color = Color.red;
                    text.color = Color.red;
                }
                else
                {
                    img.color = Color.yellow;
                    text.color = Color.yellow;
                }
            }
            else
            {
                countdownStarted = false;
            }
        }
    }

    void StartCountdown()
    {
        text = count.GetComponent<TextMeshProUGUI>();
        img = border.GetComponent<UnityEngine.UI.Image>();
        currentTime = totalTime;
        countdownStarted = true;
    }
}
