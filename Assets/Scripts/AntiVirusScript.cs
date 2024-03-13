using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiVirusScript : MonoBehaviour
{
    private float countdownRemaining = 10;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (countdownRemaining > 0) 
        {
            countdownRemaining -= Time.deltaTime;
        }
        else
        {
            //Debug.Log("ANTIVIRUS SCAN ACTIVATED");
            countdownRemaining = 10;
        }
    }
}
