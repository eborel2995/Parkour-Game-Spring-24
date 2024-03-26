using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Cheats : MonoBehaviour
{
    [SerializeField] float resize = 1;
    [SerializeField] float gravity;
    [SerializeField] MouseControl mouseControl;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravity = rb.gravityScale;
        mouseControl = GetComponent<MouseControl>();
    }

    // Update is called once per frame
    void Update()
    {
        //transform.localScale = resize;
        rb.gravityScale = gravity;


        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R pressed");
            //(toggle) player.Equip(resizer);

        }

        if (mouseControl.selectedObject != null) //player.currentlyEquiped(resizer) == true
        {
            mouseControl.selectedObject.transform.localScale = new Vector3(resize, resize, 1);
        }
        
        //fix issue of losing rb when scene changes!

    }
}
