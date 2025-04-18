using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBossFire : MonoBehaviour
{
    public PlayerStats ps;
    public float boss_hitpoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "BossFireballTrigger")
        {
            other.GetComponentInParent<BossFireballFlocking>().DeleteFireball();
            ps.DamageHealth(this.boss_hitpoint);
        }
    }
}
