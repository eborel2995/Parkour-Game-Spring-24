using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Variable to hold the transform component.
    // "[SerializeFeild]" allows these variables to be edited in Unity.
    [SerializeField] private Transform player;

    // Update is called once per frame.
    private void Update()
    {
        // Have the camera track the player's x and y position but not rotate on the z-axis with the player.
        transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
    }
}
