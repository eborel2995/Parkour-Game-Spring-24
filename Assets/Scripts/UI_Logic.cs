using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Logic : MonoBehaviour
{
    //https://www.youtube.com/watch?v=gXx_j-6z8jY

    //VisualElement root;
    // Start is called before the first frame update
    void Start()
    {
        //root = GetComponent<UIDocument>().rootVisualElement;
    }

    // Update is called once per frame
    void OnEnable()
    {
        //root.Q<Button>("buttonDrag").clicked += () => Debug.Log("Clicked Drag!");
    }

}
