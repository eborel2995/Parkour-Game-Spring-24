using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{
    Camera cam;
    [SerializeField] bool debugMode = true;
    [SerializeField] public Vector3 clickedWorldCoords = Vector3.zero;
    [SerializeField] public GameObject selectedObject = null;
    //[SerializeField] GameObject spawnableObject;

    Vector3 screenMousePos;
    Vector3 worldMousePos;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        screenMousePos = Input.mousePosition;
        worldMousePos = screenMousePos;

        //convert to world coordinates
        //worldMousePos.z = 100f;
        worldMousePos = cam.ScreenToWorldPoint(worldMousePos);

        if (debugMode)
        {
            Debug.DrawRay(transform.position, worldMousePos - transform.position, Color.blue);
        }

        //if the user clicks
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) 
        {
            Ray ray = cam.ScreenPointToRay(screenMousePos);
            RaycastHit2D hit = Physics2D.Raycast(worldMousePos, Vector2.zero);

            worldMousePos.z = 0;
            clickedWorldCoords = worldMousePos;
            if (hit.collider != null)
            {
                selectedObject = hit.collider.gameObject;
            }
            else
            { 
                //deselect the object
                selectedObject = null;
                //Debug.Log($"Clicked {hit.transform.name} at {worldMousePos}");

                //Instantiate(spawnableObject, worldMousePos, Quaternion.identity);
            }
        }

    }
}
