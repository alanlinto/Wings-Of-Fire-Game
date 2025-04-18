using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puddle : MonoBehaviour
{
  private string[] affectedUnits = { "PlayerTrigger", "MinionTrigger" };

  void OnTriggerEnter2D(Collider2D other)
  {
    if (Array.IndexOf(affectedUnits, other.gameObject.tag) != -1)
    {
      Rigidbody2D rb2d = other.GetComponentInParent<Rigidbody2D>();
      rb2d.mass *= 2f;
    }
  }

  void OnTriggerExit2D(Collider2D other)
  {
    if (Array.IndexOf(affectedUnits, other.gameObject.tag) != -1)
    {
      Rigidbody2D rb2d = other.GetComponentInParent<Rigidbody2D>();
      rb2d.mass *= 0.5f;
    }
  }
}
