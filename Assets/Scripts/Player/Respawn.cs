using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    Vector3 respawnCoords;
    [SerializeField] float deathFloorHeight;
    // Start is called before the first frame update
    void Start()
    {
        //get the default coordinates set in Unity as the respawn coordinates
        respawnCoords = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= deathFloorHeight)
        {
            Debug.Log($"{gameObject.name} fell out of the world!");

            // Make sure to zero the player's velocity and movement to prevent clipping into terrain
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(0, 0);

            //get access to playermovement and disable movement!
            //movingLeft = false;
            //movingRight = false;

            respawn();
        }
    }

    public void respawn()
    {
        //instantly teleport back to initial coordinates
        transform.position = respawnCoords;
    }
}
