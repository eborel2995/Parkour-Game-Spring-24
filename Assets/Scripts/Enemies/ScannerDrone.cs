using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerDrone : Enemy
{
    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Scanner variables.
    [SerializeField] private float chaseDistance;

    // Enum for scanner states.
    enum ScannerStates { Scan_Idle, Scan_Chase }
    ScannerStates currentScannerState;

    // Scanner states property.
    ScannerStates GetCurrentScannerState
    {
        get 
        { 
            return currentScannerState; 
        }
        set
        {
            if (currentScannerState != value)
            {
                currentScannerState = value;
            }
        }
    }

    // Start() is called before the first frame update.
    protected override void Start()
    {
        // Set state to idle.
        base.Start();
        ChangeState( ScannerStates.Scan_Idle );
    }

    // Update() is called once per frame.
    protected override void Update()
    {
        // Update state.
        base.Update();
        UpdateEnemyStates();
    }

    // Awake() is called when the script instance is being loaded.
    // Awake() is used to initialize any variables or game states before the game starts.
    protected override void Awake()
    {
        base.Awake();
    }

    // Handles scanner state updates.
    void UpdateEnemyStates()
    {
        // Distance from scanner to player.
        float _dist = Vector2.Distance(transform.position, PlayerMovement.Instance.transform.position);

        // If player instance unavailable then return.
        if (PlayerMovement.Instance == null) return;

        // Switch between scanner states.
        switch (currentScannerState)
        {
            case ScannerStates.Scan_Idle:
                if (_dist < chaseDistance)
                {
                    ChangeState(ScannerStates.Scan_Chase);
                }
                break;

            case ScannerStates.Scan_Chase:
                rb.MovePosition(Vector2.MoveTowards(transform.position, PlayerMovement.Instance.transform.position, Time.deltaTime * moveSpeed));
                FlipScanner();
                break;
        }
    }

    // Changes scanner state
    void ChangeState(ScannerStates _newState)
    {
        GetCurrentScannerState = _newState;
    }

    // Flips scanner when moving in each direction.
    void FlipScanner()
    {
        sr.flipX = PlayerMovement.Instance.transform.position.x < transform.position.x;
    }

    // Handles scanner attack.
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    }
}