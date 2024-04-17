using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{
    private Cheats cheats;
    private static MouseControl _instance;
    public static MouseControl Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<MouseControl>();
            }
            return _instance;
        }
    }

    Camera cam;
    [SerializeField] public Vector3 clickedWorldCoords = Vector3.zero;
    [SerializeField] public GameObject selectedObject = null;
    //[SerializeField] GameObject spawnableObject;

    Vector3 screenMousePos;
    Vector3 worldMousePos;

    private void Awake()
    {
        if (_instance != null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        /*else if (_instance != this)
        {
            Destroy(this);
        }*/
    }

    // Start is called before the first frame update
    void Start()
    {
        cheats = GetComponent<Cheats>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        screenMousePos = Input.mousePosition;
        worldMousePos = screenMousePos;

        //convert to world coordinates
        worldMousePos = cam.ScreenToWorldPoint(worldMousePos);

        if (cheats.debugMode)
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
            }
        }

    }
}
