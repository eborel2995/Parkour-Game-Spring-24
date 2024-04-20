using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Timer : MonoBehaviour
{
    public static Timer instance;
    public TextMeshProUGUI timeDisplay;
    private TimeSpan timeSpent;
    private bool timerEnabled;
    private float elapsedTime;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    private void Start()
    {
        timeDisplay.text = "Run Time: 00:00.00";
        timerEnabled = false;
        Timer.instance.BeginTimer();
    }

    public void BeginTimer()
    {
        timerEnabled = true;
        elapsedTime = 0f;

        StartCoroutine(UpdateTimer());
    }

    public void EndTimer()
    {
        timerEnabled = false;
    }

    private IEnumerator UpdateTimer()
    {
        while(timerEnabled)
        {
            elapsedTime += Time.deltaTime;
            timeSpent = TimeSpan.FromSeconds(elapsedTime);
            string timePlayingString = "Run Time: " + timeSpent.ToString("mm':'ss'.'ff");
            timeDisplay.text = timePlayingString;

            yield return null;
        }
    }

    
}
