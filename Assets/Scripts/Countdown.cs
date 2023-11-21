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
    private Image img;

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

                if (currentTime < totalTime / 2 && currentTime > 10)
                {
                    SetColor(Color.yellow);
                }

                if (currentTime < 10)
                {
                    SetColor(Color.red);

                    if ((int)currentTime % 2 == 0)
                    {
                        SetColor(Color.yellow);
                    }
                }
            }
            else
            {
                SetColor(Color.red);
                countdownStarted = false;
            }
        }
    }

    void StartCountdown()
    {
        text = count.GetComponent<TextMeshProUGUI>();
        img = border.GetComponent<Image>();
        currentTime = totalTime;
        countdownStarted = true;
    }

    void SetColor(Color color)
    {
        img.color = color;
        text.color = color;
    }
}
