using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalGun : MonoBehaviour
{
    [SerializeField] GameObject PortalLeft;
    [SerializeField] GameObject PortalRight;
    [SerializeField] GameObject player;
    private Vector3 clickedSpot;

    private GameObject latestPortal;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    { 
        if (Input.GetMouseButtonDown(0))
        {
            if (latestPortal != null)
            {
                Destroy(latestPortal);
            }
            clickedSpot = player.GetComponent<MouseControl>().clickedWorldCoords;
            latestPortal = Instantiate(PortalLeft, clickedSpot, Quaternion.identity);
            
        }

        if (Input.GetMouseButtonDown(1)) 
        {
            if (latestPortal != null)
            {
                Destroy(latestPortal);
            }
            clickedSpot = player.GetComponent<MouseControl>().clickedWorldCoords;
            latestPortal = Instantiate(PortalRight, clickedSpot, Quaternion.identity);
        }
    }
}
