using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ExposureTimer : MonoBehaviour
{
    public TMP_Text CountDownText;
    public float CountDownTime;

    private float midLevel = 20f;
    private float lastLevel = 10f;
    
    private readonly Color startColor = Color.black;
    private readonly Color middleColor = new Color32(255, 165, 0, 255);
    private readonly Color lastColor = Color.red;


    private float endTargetScale = 1.4f;
    private float _midTargetScale = 1.2f;
    private float defaultScale = 1f;
    private float scaleValue;
    private bool scaleUp = true;

    private bool countDownStarted;

    private void Start()
    {
        if (!GetComponentInParent<PhotonView>().IsMine)
            return;
        scaleValue = defaultScale;
        transform.rotation =
            Quaternion.LookRotation(transform.position - (GameObject.FindWithTag("MainCamera").transform.position));
        gameObject.SetActive(false);
    }

    public void StartCountDown(float duration, float midLevelStart, float lastLevelStart)
    {
        CountDownText.color = startColor;
        CountDownTime = duration;
        midLevel = midLevelStart;
        lastLevel = lastLevelStart;

        countDownStarted = true;
    }

    private void Update()
    {
        
        if (!countDownStarted)
            return;

        if (CountDownTime > 0)
        {
            CountDownTime -= Time.deltaTime;
            
            if (CountDownTime <= lastLevel)
            {
                CountDownText.color = lastColor;
                if (CountDownTime < 1)
                {
                    CountDownText.text = CountDownTime.ToString("0.0");
                }
                else
                {
                    CountDownText.text = CountDownTime.ToString("#.0");
                }

                if (scaleValue >= endTargetScale)
                {
                    scaleUp = false;
                } 
                else if (scaleValue < defaultScale)
                {
                    scaleUp = true;
                }
                scaleValue = scaleUp ? scaleValue + 0.005f : scaleValue - 0.005f;

                CountDownText.gameObject.transform.localScale = new Vector3(scaleValue, scaleValue, 1);
            }
            else if (CountDownTime <= midLevel)
            {
                CountDownText.color = middleColor;
                CountDownText.text = CountDownTime.ToString("#");

                if (scaleValue >= _midTargetScale)
                {
                    scaleUp = false;
                } 
                else if (scaleValue < defaultScale)
                {
                    scaleUp = true;
                }
                scaleValue = scaleUp ? scaleValue + 0.0025f : scaleValue - 0.0025f;

                CountDownText.gameObject.transform.localScale = new Vector3(scaleValue, scaleValue, 1);
            }
            else
            {
                CountDownText.text = CountDownTime.ToString("#");
            }
        }
        else
        {
            CountDownText.text = "";
        }
    }
}
