using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampFire : MonoBehaviour
{
  public GameObject player;
  private float timer = 5f;

  void Start()
  {
    player = GameObject.Find("Player");
  }

  private void FixedUpdate()
  {
    timer += Time.deltaTime;
  }

  void OnTriggerStay2D(Collider2D other)
  {
    if (timer > 5f)
    {
      if (other.tag == "PlayerTrigger")
      {
        timer = 0f;
        player.GetComponent<PlayerStats>().HealHealth(5);
      }
    }
  }
}
