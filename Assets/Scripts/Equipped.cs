using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Equipped : MonoBehaviour
{
    [SerializeField] public Tool EquippedTool;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (Event.current.keyCode)
        {
            case KeyCode.E:
                Debug.Log("Select");
                break;
            case KeyCode.R:
                Debug.Log("Resize");
                break;
            case KeyCode.Q:
                Debug.Log("Duplicate");
                break;
            case KeyCode.Z:
                Debug.Log("Delete");
                break;
            case KeyCode.F:
                Debug.Log("Freeze");
                break;
            case KeyCode.V:
                Debug.Log("Visibility");
                break;
            default:
                break;
        }
        
    }
}
