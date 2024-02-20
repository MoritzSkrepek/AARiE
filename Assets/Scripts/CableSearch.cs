using System.Collections;
using System.Diagnostics;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CableSearch : MonoBehaviour
{
    public TextMeshPro userInfo;

    private float timeRemaining = 6.0f;
    private bool objectPlaced = false;
    private bool canStartScript = false;

    public GameObject Plane;

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2.0f);
        canStartScript = true;
    }

    void Start()
    {
        userInfo.text = "Schauen Sie auf das Rote Kabel";
        StartCoroutine(DelayedStart());
    }

    void Update()
    {
        if (canStartScript && !objectPlaced)
        {
            if (timeRemaining > 1f)
            {
                timeRemaining -= Time.deltaTime; 
                int elapsedTime = Mathf.FloorToInt(timeRemaining % 60);
                userInfo.text = elapsedTime.ToString();
            }
            else
            {
                timeRemaining = 1f;

                userInfo.text = "";
                placeFound();
                objectPlaced = true;
            }
        }
       
    }

    private void placeFound()
    {
        Plane.GetComponent<PictureTakingButton>().takingPicture();
    }
}
