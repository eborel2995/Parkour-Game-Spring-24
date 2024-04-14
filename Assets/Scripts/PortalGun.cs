using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PortalGun : MonoBehaviour
{
    [SerializeField] GameObject PortalLeft;
    [SerializeField] GameObject PortalRight;
    [SerializeField] GameObject player;
    private Vector3 clickedSpot;

    private GameObject latestBluePortal;
    private GameObject latestOrangePortal;
    private Vector3 BluePortalCoords;
    private Vector3 OrangePortalCoords;

    private int teleportCooldown = 3;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        //consolidate these into a function

        //blue portal
        if (Input.GetMouseButtonDown(0))
        {
            if (latestBluePortal != null)
            {
                Destroy(latestBluePortal);
            }
            clickedSpot = player.GetComponent<MouseControl>().clickedWorldCoords;
            latestBluePortal = Instantiate(PortalLeft, clickedSpot, Quaternion.identity);
            BluePortalCoords = clickedSpot;
            
        }

        //orange portal
        if (Input.GetMouseButtonDown(1)) 
        {
            if (latestOrangePortal != null)
            {
                Destroy(latestOrangePortal);
            }
            clickedSpot = player.GetComponent<MouseControl>().clickedWorldCoords;
            latestOrangePortal = Instantiate(PortalRight, clickedSpot, Quaternion.identity);
            OrangePortalCoords = clickedSpot;
        }

        if (teleportCooldown == 3)
        {
            teleportCollision();
        }

    }


    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(1);
        teleportCooldown--;
    }
    public void teleportCooldownTimer()
    {
        StartCoroutine(CountDown());
    }


    void teleportCollision()
    {
        //if (player collidesWith latestBluePortal && latestOrangePortal != null)
        {
            player.transform.position = latestOrangePortal.transform.position;
            teleportCooldownTimer();
        }
        //else if (player collidesWith latestOrangePortal && latestBluePortal != null)
        {
            player.transform.position = latestBluePortal.transform.position;
            teleportCooldownTimer();
        }
    }
}


