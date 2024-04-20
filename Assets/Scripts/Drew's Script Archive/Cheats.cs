using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Cheats : MonoBehaviour
{
    // DREW WANTS TO ARCHIVE THIS SCRIPT FOR HIS PERSONAL USE!

    /*
    [SerializeField] public bool debugMode = true;
    [SerializeField] float resize = 1;
    [SerializeField] float gravity = 5;
    [SerializeField] MouseControl mouseControl;
    [SerializeField] public bool invincible = false;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody2D>();
        //gravity = rb.gravityScale;
        mouseControl = MouseControl.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseControl.selectedObject != null) //player.currentlyEquiped(resizer) == true
        {
            mouseControl.selectedObject.transform.localScale = new Vector3(resize, resize, 1);

            try
            {
                rb = mouseControl.selectedObject.GetComponent<Rigidbody2D>();
                rb.gravityScale = gravity;
            }
            catch 
            {
                Debug.Log($"Cheats.cs: {gameObject.name} doesn't have a rigidbody component");
            }
            
        }
    }
    */
}
