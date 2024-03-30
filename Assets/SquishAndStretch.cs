using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquishAndStretch : MonoBehaviour
{
    private float baseScale = 1.0f;
    private float baseAnimationSpeed = 1.0f;

    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        baseScale = transform.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        float currentScale = transform.localScale.x; // Assuming uniform scale
        UpdateAnimationSpeed(currentScale);
    }

    void UpdateAnimationSpeed(float currentScale)
    {
        float animationSpeedRatio = 1.0f / (currentScale / baseScale);
        float currentAnimationSpeed = baseAnimationSpeed * animationSpeedRatio;
        anim.speed = currentAnimationSpeed;
    }
}
