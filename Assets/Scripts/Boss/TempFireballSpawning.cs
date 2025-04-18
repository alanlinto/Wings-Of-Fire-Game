using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempFireballSpawning : MonoBehaviour {
    public GameObject fireballPrefab;

    void Start() {
        for (int i = 0; i < 100; i++) {
            Vector2 spawnPosition = new Vector2(Random.Range(-8f, 8f), Random.Range(-8f, 8f));
            GameObject fireball = Instantiate(
                fireballPrefab, 
                spawnPosition, 
                Quaternion.identity,
                this.transform
            );
            Vector2 initialVelocity = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * 5;

            BossFireballFlocking flockingScript = fireball.GetComponent<BossFireballFlocking>();
            flockingScript.initialVelocity = initialVelocity;
            flockingScript.fireballsParent = this.gameObject;
        }
    }

    void Update() {
        
    }
}
