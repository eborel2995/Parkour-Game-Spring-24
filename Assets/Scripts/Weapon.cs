using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : ScriptableObject
{
    private string name;
    private float damage;

    // Constructor
    public Weapon(string n, float d)
    {
        name = n;
        damage = d;
    }

    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
