using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampFireGen : MonoBehaviour
{
    public int width, height, spawners;
    public float MinimumSpacingDistance;
    public List<Vector2> positions;
    public GameObject campfire;
    public LayerMask obstacle, puddle;

    static CampFireGen instance;

    void Awake()
    {
        instance = this;
        UnityEngine.Random.InitState(MainMenuController.seed);
    }

    public static void Generate() {
        instance.CreateSpawnPoints();
    }

    void CreateSpawnPoints() {
        for (int i = 0; i < spawners; ++i) {
            Vector2 newPos = new Vector2(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));

            if (!EnoughSpace(newPos)) {
                --i;
                continue;
            }

            if (Physics2D.OverlapCircle(newPos, 2, obstacle) == null && Physics2D.OverlapCircle(newPos, 2, puddle) == null) {
                positions.Add(newPos);
                Instantiate(campfire, newPos, Quaternion.identity, this.transform);
            } else {
                --i;
            }
        }
        
    }

    // Check that there isn't another campfire located too close
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
}


