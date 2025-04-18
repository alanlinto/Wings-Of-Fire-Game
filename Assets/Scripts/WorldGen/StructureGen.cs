using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureGen : MonoBehaviour
{
    public int width, height, structures, retries;
    public float MinimumSpacingDistance;
    public GameObject tree, stone, pillar, well;
    public LayerMask obstacle, puddle;

    List<(Vector2, int)> positions = new List<(Vector2, int)>();
    Vector3[] edgePoints;
    int decorationIndex;

    static StructureGen instance;

    void Awake()
    {
        instance = this;
        UnityEngine.Random.InitState(MainMenuController.seed);
        SetUpEdgePoints();
    }

    public static void Generate() {
        instance.CreateSpawnPoints();
    }

    void CreateSpawnPoints() {
        for (int i = 0; i < structures; ++i) {
            Vector2 newPos = GetNewPosition();
            bool depleted = false;
            decorationIndex = UnityEngine.Random.Range(0,4);
            GameObject decoration = null;
            
            for (int attempts = 0; attempts < retries; ++attempts) {
                if (EnoughSpace(newPos)) {
                    break;
                } else if (attempts == retries - 1) {
                    depleted = true;
                } 
                else {
                    newPos = GetNewPosition();
                }
            }

            if (depleted) {
                continue;
            }

            switch (decorationIndex)
            {
                case 0:
                    decoration = tree;
                    break;

                case 1:
                    decoration = stone;
                    break;
                
                case 2:
                    decoration = pillar;
                    break;
                
                case 3:
                    decoration = well;
                    break;

                default:
                    Debug.Log("Random number out of range");
                    break;
            }
            // If spawning a tree, make sure it isn't submerged in puddles
            if (decorationIndex == 0) {
                for (int attempts = 0; attempts < retries; ++attempts) {
                    if (!IsSubmerged(newPos) && EnoughSpace(newPos)) {
                        positions.Add((newPos, decorationIndex));
                        Instantiate(decoration, newPos, Quaternion.identity, this.transform);
                        break;
                    } else {
                        newPos = GetNewPosition();
                    }
                }
            }
            // If spawning a well, make sure it submerged in puddles (replicate the idea of a leaking well)
            else if (decorationIndex == 3) {
                for (int attempts = 0; attempts < retries; ++attempts) {
                    if (IsSubmerged(newPos) && EnoughSpace(newPos)) {
                        positions.Add((newPos, decorationIndex));
                        Instantiate(decoration, newPos, Quaternion.identity, this.transform);
                        break;
                    } else {
                        newPos = GetNewPosition();
                    }
                }
            }
            else {
                for (int attempts = 0; attempts < retries; ++attempts) {
                    if (EnoughSpace(newPos)) {
                        positions.Add((newPos, decorationIndex));
                        Instantiate(decoration, newPos, Quaternion.identity, this.transform);
                        break;
                    } else {
                        newPos = GetNewPosition();
                    }
                }
            }
        }
        
    }

    Vector2 GetNewPosition() {
        return new Vector2(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
    }

    bool IsSubmerged(Vector2 position) {
        foreach (Vector3 point in edgePoints) {
            Vector3 pointToCheck = (Vector3)position + point;
            Collider2D collider = Physics2D.OverlapPoint(pointToCheck, puddle);

            if (collider == null) {
                return false;
            } 
        }

        return true;
    }

    void SetUpEdgePoints() {
        edgePoints = new Vector3[9];
        edgePoints[0] = Vector2.zero;

        for (int i = 1; i < edgePoints.Length; ++i) {
            float degrees = (i - 1) * 45f;
            float x = Mathf.Cos(degrees * Mathf.Deg2Rad);
            float y = Mathf.Sin(degrees * Mathf.Deg2Rad);
            edgePoints[i] = new Vector3(x, y);
        }
    }

    bool EnoughSpace(Vector2 position) {

        float minDistance = Mathf.Infinity;
        int indexD = 0;
        
        foreach ((Vector2 location, int decoration) in positions) {
            float distance = (location - position).sqrMagnitude;

            if (distance < minDistance) {
                minDistance = distance;
                indexD = decoration;
            }
        }

        // Encourage structures of the same type to spawn in clusters
        bool sameDecoration = positions.Count == 0 ? false : indexD == decorationIndex;
        float checkRadius = (MinimumSpacingDistance * MinimumSpacingDistance) / (sameDecoration ? 4f : 1f);

        return Physics2D.OverlapCircle(position, MinimumSpacingDistance /(sameDecoration ? 2f : 1f), obstacle) == null && minDistance > checkRadius;
    }

}
