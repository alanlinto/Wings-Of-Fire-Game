using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : MonoBehaviour {
    public float initialHealth;
    public InGameUI gameUI;

    float health;

    BossStateMachine stateMachine;
    Animator animator;
    Animator deathExplosionAnimator;
    CameraFollow cameraScript;

    void Start() {
        animator = this.GetComponent<Animator>();
        stateMachine = this.GetComponent<BossStateMachine>();
        deathExplosionAnimator = this.transform.GetChild(2).GetComponent<Animator>();

        health = initialHealth;
    }

    public void DamageHealth(float hitPoints) {
        // Ignore damage if already dead.
        if (health <= 0) { return; } 

        if (hitPoints < 0) {
            throw new Exception("Cannot do negative damage");
        }
        health -= hitPoints;

        if (health <= 0) {
            health = 0;
            animator.SetBool("IsDead", true);
            deathExplosionAnimator.SetTrigger("IsDead");
            gameUI.OnBossDeath();
            stateMachine.SetState(BossStates.Dead);
            if (cameraScript != null) {
                cameraScript.OnBossDeath();
            }
        }

        gameUI.SetBossHealthPercentage(health / initialHealth);
    }

    public void ProvideCamera(CameraFollow cameraScript) {
        this.cameraScript = cameraScript;
    }

    public bool IsAlive() { 
        return health > 0; 
    }
}
