using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    //private GameObject player;
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovement pm;
    private Cheats cheats;

    public Image healthBar;
    private static float baseHealth = 100f;
    public float healthAmount = baseHealth;

    private Vector3 respawnCoords;
    [SerializeField] private float deathFloorHeight = -80;

    private PlayerStatesList pList;
    // Start is called before the first frame update
    void Start()
    {
        //get the default coordinates set in Unity as the respawn coordinates
        respawnCoords = transform.position;

        //player = GameObject.Find("Player");
        anim    = GetComponent<Animator>();
        rb      = GetComponent<Rigidbody2D>();
        pm      = GetComponent<PlayerMovement>();
        cheats  = GetComponent<Cheats>();
        pList   = GetComponent<PlayerStatesList>();
    }

    // Update is called once per frame
    void Update()
    {
        //if the entity runs out of health
        if (healthAmount <= 0)
        {
            Debug.Log($"{gameObject.name} ran out of health!");
            Die();
            //delay to play death animation
            Invoke(nameof(Respawn), 3f);
            //Respawn();
            //RestartLevel();
        }

        if (gameObject.name == "Player")
        {
            if (cheats.debugMode == true)
            {
                //FOR TESTING PURPOSES
                if (Input.GetKeyDown(KeyCode.Return))
                { TakeDamage(20); }

                if (Input.GetKeyDown(KeyCode.Backspace))
                { Heal(10); }
                //////////////////////
            }
        }
        

        if (transform.position.y <= deathFloorHeight)
        {
            Debug.Log($"{gameObject.name} fell out of the world!");

            // Make sure to zero the player's velocity and movement to prevent clipping into terrain
            //Rigidbody2D rb = GetComponent<Rigidbody2D>(); //remove this
            rb.velocity = new Vector2(0, 0);

            TakeDamage(healthAmount);
        }

        if (healthBar != null)
        {
            UpdateHealthbar();
        }
        
    }

    public void TakeDamage(float amount)
    {
        healthAmount -= amount;
    }

    public void Heal(float amount)
    {
        healthAmount += amount;
        healthAmount = Mathf.Clamp(healthAmount, 0, 100);
    }

    public void UpdateHealthbar()
    {
        healthBar.fillAmount = healthAmount / 100f;
        //Debug.Log($"{gameObject.name} has {healthAmount} health");
    }

    public void Die()
    {
        // Activate death animation.
        anim.SetTrigger("death");
        pList.alive = false;
        if (gameObject.name == "Player")
        {
            // Disable player movement.
            pm.ignoreUserInput = true;
        }

    }

    public void Respawn()
    {
        //instantly teleport back to initial coordinates
        transform.position = respawnCoords;
        healthAmount = baseHealth;
        pm.ignoreUserInput = false;
        pList.alive = true;
        Time.timeScale = 1;
    }

    
    private void RestartLevel()
    {
        // Restart level.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
}
