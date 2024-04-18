using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerDrone : Enemy
{
    [SerializeField] private float chaseDistance;

    enum ScannerStates
    {
        Scan_Idle,
        Scan_Chase
    }

    ScannerStates currentScannerState;

    ScannerStates GetCurrentScannerState
    {
        get { return currentScannerState; }
        set
        {
            if (currentScannerState != value)
            {
                currentScannerState = value;
            }
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        ChangeState( ScannerStates.Scan_Idle );
    }
    protected override void Update()
    {
        base.Update();
        UpdateEnemyStates();
    }
    protected override void Awake()
    {
        base.Awake();
    }
    void UpdateEnemyStates()
    {
        float _dist = Vector2.Distance(transform.position, PlayerMovement.Instance.transform.position);
        if (PlayerMovement.Instance == null) return;
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
    void ChangeState(ScannerStates _newState)
    {
        GetCurrentScannerState = _newState;
    }
    void FlipScanner()
    {
        sr.flipX = PlayerMovement.Instance.transform.position.x < transform.position.x;
    }
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    }
}
