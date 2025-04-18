using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : MonoBehaviour
{
    GameObject Player;

    void Start()
    {
        Player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 playerAbove = new Vector3(transform.position.x, transform.position.y, -1);
        Vector3 playerBelow = new Vector3(transform.position.x, transform.position.y, 1);

        transform.position = Player.transform.position.y > transform.position.y ? playerAbove : playerBelow;

    }
}
