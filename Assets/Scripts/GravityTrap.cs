using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GravityTrap : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float force = 1;
    [SerializeField] float range = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(player.transform.position, transform.position) < range)
        {
            Attract(player, force);
            player.GetComponent<Rigidbody2D>().gravityScale = 0;
        }
        else
        {
            player.GetComponent<Rigidbody2D>().gravityScale = 5;
        }
    }

    /// <summary>
    /// Apply an attraction force that pulls the GameObject towards this current object
    /// </summary>
    /// <param name="go"></param>
    /// <param name="force">Higher value applies a stronger attraction force</param>
    private void Attract(GameObject go, float force)
    {
        //float angle = Vector3.Angle(go.transform.position, transform.position);

        float xDif = go.transform.position.x - transform.position.x;
        float yDif = go.transform.position.y - transform.position.y;
        Vector3 vec = new Vector3(xDif, yDif, 0);

        go.transform.position -= force * Time.deltaTime * vec;
        
    }
}
