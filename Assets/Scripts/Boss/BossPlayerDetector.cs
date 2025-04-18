using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPlayerDetector : MonoBehaviour {
    public GameObject boss;
    // public GameObject bossWall;
    public InGameUI gameUI;
    
    BossStateMachine bossFSA;

    void Start() {
        bossFSA = boss.GetComponent<BossStateMachine>();
        // if (bossWall != null) {
        //     bossWall.SetActive(false);
        // }
    }

    public void setPosition()
    {
        gameObject.transform.position = boss.transform.position;
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "PlayerTrigger") {
            bossFSA.OnPlayerEnteredArena();
            // if (bossWall != null) {
            //     bossWall.SetActive(true);
            // }
            if (gameUI != null) {
                gameUI.OnEnterBossArena();
            }
        }
    }
    void OnTriggerExit2D(Collider2D other) {
        // There is an intentional gap between the detector collider and the 
        // wall. So stop the boss attack, but keep the wall up.
        if (other.tag == "PlayerTrigger") {
            bossFSA.OnPlayerLeftArena();
            if (gameUI != null) {
                gameUI.OnLeaveBossArena();
            }
        }
    }
}
