using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossStates {
    Idle,
    AttackCooldown,
    SpiralAttack,
    StarAttack,
    Dead
}

public class BossStateMachine : MonoBehaviour {
    BossStates currentState;

    // General variables
    public GameObject player, phoenix;

    // Variables for AttackCooldown
    public float cooldownDuration;
    float goToAttackTimer;

    // Variables shared for both attack states
    float goToCooldownTimer;
    public GameObject fireballPrefab;
    public GameObject fireballsParent;
    public Vector2 fireballSpawnOffset;
    public float chanceOfStar;

    // Variables for SpiralAttack
    public float spiralDuration;
    public int fireballsPerSpiral;
    public float spiralInitialSpeed;
    public int numberOfSpirals;
    float spawnFireballTimer;

    // Variables for StarAttack
    public float starDuration;
    public int starsPerAttack;
    public int fireballsPerStar;
    public float starInitialSpeed;
    public float starNextOffsetPercentage;
    float spawnStarTimer;
    float starAngleOffset;

    void Start() {
        SetState(BossStates.Idle);
    }

    public void SetState(BossStates state) {
        currentState = state;

        if (currentState == BossStates.Idle) {
            // Nothing to setup here.
        }
        else if (currentState == BossStates.AttackCooldown) {
            goToAttackTimer = cooldownDuration;
        }
        else if (currentState == BossStates.SpiralAttack) {
            goToCooldownTimer = spiralDuration;
            spawnFireballTimer = 0;
        }
        else if (currentState == BossStates.StarAttack) {
            goToCooldownTimer = starDuration;
            spawnStarTimer = 0;
            starAngleOffset = 0;
        }
        else if (currentState == BossStates.Dead) {
            // Nothing to setup here.
        }
        else {
            throw new Exception("Unrecognized state");
        }
    }

    void FixedUpdate() {
        if (currentState == BossStates.Idle) {
            DoIdle();
        }
        else if (currentState == BossStates.AttackCooldown) {
            DoAttackCooldown();
        }
        else if (currentState == BossStates.SpiralAttack) {
            DoSpiralAttack();
        }
        else if (currentState == BossStates.StarAttack) {
            DoStarAttack();
        }
        else if (currentState == BossStates.Dead) {
            DoDead();
        }
        else {
            throw new Exception("Unrecognized state");
        }
    }

    void DoIdle() {
        // Do nothing.
    }

    void DoAttackCooldown() {
        goToAttackTimer -= Time.fixedDeltaTime;
        if (goToAttackTimer <= 0) {
            if (UnityEngine.Random.Range(0,1f) < chanceOfStar) {
                SetState(BossStates.StarAttack);
            }
            else {
                SetState(BossStates.SpiralAttack);
            }
        }
    }

    void DoSpiralAttack() {
        goToCooldownTimer -= Time.fixedDeltaTime;
        if (goToCooldownTimer <= 0) {
            SetState(BossStates.AttackCooldown);
        }

        spawnFireballTimer -= Time.fixedDeltaTime;
        while (spawnFireballTimer <= 0) {
            float percentageThroughAnimation = 1 - (goToCooldownTimer / spiralDuration);
            float angle = percentageThroughAnimation * numberOfSpirals * Mathf.PI * 2;
            Vector2 initialVelocity = new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ) * spiralInitialSpeed;

            CreateFireball(initialVelocity);

            spawnFireballTimer += spiralDuration / (fireballsPerSpiral - 1);
        }
    }

    void DoStarAttack() {
        goToCooldownTimer -= Time.fixedDeltaTime;
        if (goToCooldownTimer <= 0) {
            SetState(BossStates.AttackCooldown);
        }

        spawnStarTimer -= Time.fixedDeltaTime;
        while (spawnStarTimer <= 0) {
            for (int i = 0; i < fireballsPerStar; i++) {
                float angle = Mathf.PI * 2 * i / fireballsPerStar;
                Vector2 initialVelocity = new Vector2(
                    Mathf.Cos(angle + starAngleOffset),
                    Mathf.Sin(angle + starAngleOffset)
                ) * starInitialSpeed;
                
                CreateFireball(initialVelocity);
            }

            starAngleOffset += Mathf.PI * 2 / fireballsPerStar * starNextOffsetPercentage;
            spawnStarTimer += starDuration / (starsPerAttack - 1);
        }
    }

    void DoDead() {
        // Do nothing.
    }

    Vector2 GetFireballSpawnPosition() {
        return (Vector2) phoenix.transform.position + fireballSpawnOffset;
    }

    GameObject CreateFireball(Vector2 initialVelocity) {
        Vector2 spawnPosition = GetFireballSpawnPosition();

        GameObject fireball = Instantiate(
            fireballPrefab, 
            spawnPosition, 
            Quaternion.identity,
            fireballsParent.transform
        );

        BossFireballFlocking flockingScript = fireball.GetComponent<BossFireballFlocking>();
        flockingScript.initialVelocity = initialVelocity;
        flockingScript.fireballsParent = fireballsParent;

        return fireball;
    }

    public void OnPlayerEnteredArena() {
        if (currentState != BossStates.Dead) {
            SetState(BossStates.AttackCooldown);
        }
    }

    public void OnPlayerLeftArena() {
        if (currentState != BossStates.Dead) {
            SetState(BossStates.Idle);
        }
    }
}
