using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawning : MonoBehaviour {

    public int width, height, spawners;
    public List<Vector2> positions;
    public GameObject[] minionPrefabs;

    public GameObject player, boss;
    public LayerMask obstacle;
    public float MinimumPlayerDistance, MaximumPlayerDistance, IdealMinionCount, spawnAttemptInterval, MinimumSpacingDistance;
    float spawnAttemptTimer = 0;

    static MinionSpawning instance;

    void Awake()
    {
        instance = this;
        UnityEngine.Random.InitState(MainMenuController.seed);
    }

    public static void Generate() {
        instance.CreateSpawnPoints();
    }

    void FixedUpdate() {
        spawnAttemptTimer -= Time.fixedDeltaTime;
        if (spawnAttemptTimer < 0) {
            spawnAttemptTimer = spawnAttemptInterval;

            float currentMinionCount = GameObject.FindGameObjectsWithTag("Minion")
                .Where(x => !x.GetComponent<Minion>().IsAlly)
                .ToArray()
                .Length;

            if (currentMinionCount < IdealMinionCount) {
                var availableLocations = positions.Where(p => {
                    float dist = ((Vector2) player.transform.position - p).magnitude;
                    return dist > MinimumPlayerDistance && dist < MaximumPlayerDistance;
                }).ToArray();

                if (availableLocations.Length > 1) {
                    var selected = availableLocations[UnityEngine.Random.Range(0, availableLocations.Length)];
                    var selectedMinion = minionPrefabs[UnityEngine.Random.Range(0, minionPrefabs.Length)];
                    var minion = Instantiate(selectedMinion, selected, Quaternion.identity, this.transform);
                    
                    var minionMovementScript = minion.GetComponent<MinionMovement>();
                    minionMovementScript.target = player;
                    
                    var minionScript = minion.GetComponent<Minion>();
                    minionScript.player = player;
                    minionScript.boss = boss;
                }
            }
        }   
    }

    void CreateSpawnPoints() {
        for (int i = 0; i < spawners; ++i) {
            Vector2 newPos = new Vector2(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));

            if (!EnoughSpace(newPos)) {
                --i;
                continue;
            }

            if (Physics2D.OverlapBox(newPos, Vector2.one * 2, obstacle) == null) {
                positions.Add(newPos);
            } else {
                --i;
            }
        }
    }

    bool EnoughSpace(Vector2 spawnLocation) {

        float minDistance = Mathf.Infinity;

        if (positions.Count == 0) {
            return true;
        }

        foreach (Vector2 location in positions) {
            float distance = (location - spawnLocation).sqrMagnitude;

            if (distance < minDistance) {
                minDistance = distance;
            }
        }

        return minDistance > MinimumSpacingDistance * MinimumSpacingDistance;
    }

    void OnDrawGizmosSelected() {
        foreach (var position in positions) {
            Gizmos.DrawIcon(position, "Skull.png", false);
        }
    }
}
