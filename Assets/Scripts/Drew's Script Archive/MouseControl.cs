using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{
    // DREW WANTS TO ARCHIVE THIS SCRIPT FOR HIS PERSONAL USE!

    /*
    //...
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

    //...
    Camera cam;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    [SerializeField] public Vector3 clickedWorldCoords = Vector3.zero;
    [SerializeField] public GameObject selectedObject = null;

    //...
    Vector3 screenMousePos;
    Vector3 worldMousePos;

    // Awake() is called when the script instance is being loaded.
    // Awake() is used to initialize any variables or game states before the game starts.
    private void Awake()
    {
        if (_instance != null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start() is called before the first frame update.
    void Start()
    {
        cam = Camera.main;
    }

    // Update() is called once per frame.
    void Update()
    {
        screenMousePos = Input.mousePosition;
        worldMousePos = screenMousePos;

        //convert to world coordinates
        worldMousePos = cam.ScreenToWorldPoint(worldMousePos);

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
                // Deselect the object.
                selectedObject = null;
            }
        }
    }
    */
}
