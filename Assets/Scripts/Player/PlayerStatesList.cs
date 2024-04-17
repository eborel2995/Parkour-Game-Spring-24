using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatesList : MonoBehaviour
{
    public bool alive = true;
    public bool jumping = false;
    public bool dashing = false;
    public bool recoilingX, recoilingY;
    public bool lookingRight;
    public bool invincible; //remove, this is in Cheats.cs
}
