using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{
    Camera cam;
    [SerializeField] bool debugMode = true;
    [SerializeField] private Vector3 clickedWorldCoords = Vector3.zero;
    [SerializeField] GameObject spawnableObject;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 screenMousePos = Input.mousePosition;
        Vector3 worldMousePos = screenMousePos;

        //convert to world coordinates
        //worldMousePos.z = 100f;
        worldMousePos = cam.ScreenToWorldPoint(worldMousePos);

        if (debugMode)
        {
            Debug.DrawRay(transform.position, worldMousePos - transform.position, Color.blue);
        }

        //if the user clicks
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = cam.ScreenPointToRay(screenMousePos);
            RaycastHit2D hit = Physics2D.Raycast(worldMousePos, Vector2.zero);

            if (hit.collider != null) 
            {
                worldMousePos.z = 0;
                clickedWorldCoords = worldMousePos;
                Debug.Log($"Clicked {hit.transform.name} at {worldMousePos}");

                Instantiate(spawnableObject, worldMousePos, Quaternion.identity);
            }
        }

    }
}
