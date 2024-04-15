using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Image healthBar;
    public float healthAmount = 100f; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (healthAmount <= 0)
        {
            Application.LoadLevel(Application.loadedLevel); //restart level
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            TakeDamage(20);
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Heal(10);
        }
    }

    public void TakeDamage(float amount)
    {
        healthAmount -= amount;
        healthBar.fillAmount = healthAmount / 100f;
    }

    public void Heal(float amount)
    {
        healthAmount += amount;
        healthAmount = Mathf.Clamp(healthAmount, 0, 100);

        healthBar.fillAmount = healthAmount / 100f;
    }
}
