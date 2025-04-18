using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour {
    [SerializeField]
    float playerHealth;
    public Animator animator;
    public HealthBar hBar;

    public int allyLimit;

    public GameObject trigger;
    CameraFollow cameraScript;
    public InGameUI gameUI;

    int currentAllyCount;

    float initialHealth;

    void Start() {
        initialHealth = playerHealth;
        if (this.hBar != null || this.gameUI != null) {
            gameUI.SetupMinionMeter(allyLimit, allyLimit);
            gameUI.SetPlayerHealthPercentage(1);
            this.CheckAllyCount();
        }
    }

    public void DamageHealth(float hitPoint) {
        this.playerHealth -= hitPoint;

        if (this.playerHealth <= 0) {
            this.playerHealth = 0;
            
            // Death Animation
            animator.SetBool("WalkingUp", false);
            transform.Rotate(0, 0, transform.rotation.y == 180f ? -90f : 90f);

            // Disable the trigger (so nothing collides with the player anymore).
            trigger.SetActive(false);

            // Do the camera effect.
            if (cameraScript != null) {
                cameraScript.OnPlayerDeath();
            }
            
            // Hide the boss health bar and minion meter.
            gameUI.OnPlayerDeath();
        }

        gameUI.SetPlayerHealthPercentage(playerHealth / initialHealth);
    }

    public void HealHealth(float hitPoint) {
        this.playerHealth += hitPoint;

        this.playerHealth = Mathf.Clamp(this.playerHealth, 0, 100);

        gameUI.SetPlayerHealthPercentage(playerHealth / initialHealth);
    }

    public bool IsAlive() {
        return this.playerHealth > 0;
    }

    public void CheckAllyCount() {
        currentAllyCount = GameObject.FindGameObjectsWithTag("Minion")
            .Where(x => {
                Minion minionScript = x.GetComponent<Minion>();
                return minionScript.IsAlive() && minionScript.IsAlly;
            })
            .Count();
        gameUI.SetMinionMeterValue(allyLimit - currentAllyCount, true);
    }
    public bool CanFormAlliance() {
        CheckAllyCount();
        return currentAllyCount < allyLimit;
    }

    public void ProvideCamera(CameraFollow cameraScript) {
        this.cameraScript = cameraScript;
    }
}
