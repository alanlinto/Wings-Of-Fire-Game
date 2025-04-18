using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{

    Vector2 randomSpawn()
    {
        Vector2 spawnPos = new Vector2(Random.Range(1.5f, 90f), Random.Range(5f, 95f));
        Collider2D checkRadius = Physics2D.OverlapCircle(spawnPos, 5.0f, LayerMask.GetMask("WallSolid"));  //Detects if there is any walls within the player's range

        if (checkRadius != null)
        {
            spawnPos = randomSpawn();
        }

        return spawnPos;
    }

    public void Generate()
    {
        this.gameObject.transform.position = this.randomSpawn();
    }

}
