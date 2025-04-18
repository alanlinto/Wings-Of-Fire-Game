using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MinionStates
{
  Wander,
  Seeking,
  Attack,
  Following,
  Dead
}

public class Minion : MonoBehaviour
{

  public MinionStates currentState;
  public GameObject player, boss, target, indicator, trigger;
  public Animator animator;
  public SpriteRenderer sp;
  public Rigidbody2D rb2d;
  public bool IsAlly = false;
  public float attackRange = 2f;
  public int attackDamage = 10;
  public float initialHealth = 12;
  public float detectionRange = 50f;
  public float wanderDistance = 30f;
  public float followDistance = 20f;
  public float bossFireballDamage = 1;
  public float otherMinionAttackDamage = 2;
  public float playerSpellDamage = 4;
  private float health;
  private float attackCooldown = 0f;
  private PlayerStats playerStats;
  private BossHealth bossStats;
  public Unit pathfinder;
  public MinionMovement minionMovement;
  public string allyLayerName;
  public LayerMask allyLayer;

  void Start()
  {
    health = initialHealth;
    indicator = transform.Find("Indicator").gameObject;
    playerStats = player.GetComponent<PlayerStats>();
    bossStats = boss.GetComponent<BossHealth>();

    SetState(MinionStates.Wander);
  }

  public void SetState(MinionStates state)
  {
    currentState = state;

    if (currentState == MinionStates.Wander)
    {
      pathfinder.StopAllCoroutines();
      pathfinder.CoroutineActive = false;
      minionMovement.Wander();

    }
    else if (currentState == MinionStates.Seeking)
    {
      minionMovement.target = target;
      pathfinder.target = target.transform;
      pathfinder.StopCoroutine(pathfinder.RefreshPath());
      pathfinder.StartCoroutine(pathfinder.RefreshPath());

    }
    else if (currentState == MinionStates.Attack)
    {
      animator.SetTrigger("Attack");
      if (IsAlly)
      {
        bossStats.DamageHealth(attackDamage);
      }
      else
      {
        if (target.tag == "Minion")
        {
          target.GetComponent<Minion>().DamageHealth(otherMinionAttackDamage);
        }
        else
        {
          playerStats.DamageHealth(attackDamage);
        }
      }
      attackCooldown = 0f;

    }
    else if (currentState == MinionStates.Following)
    {
      minionMovement.target = player;
      pathfinder.target = player.transform;
      pathfinder.StopCoroutine(pathfinder.RefreshPath());
      pathfinder.StartCoroutine(pathfinder.RefreshPath());

    }
    else if (currentState == MinionStates.Dead)
    {
      if (!IsAlly && playerStats.CanFormAlliance())
      {
        health = initialHealth;
        IsAlly = true;
        trigger.layer = LayerMask.NameToLayer(allyLayerName);
        indicator.GetComponent<Indicator>().changeIndicator();

        target = null;
        pathfinder.StopAllCoroutines();
        pathfinder.CoroutineActive = false;
      }
      else
      {
        animator.SetTrigger("Died");
        rb2d.bodyType = RigidbodyType2D.Static;
        Destroy(this.gameObject, 2f);
      }
      playerStats.CheckAllyCount();

    }
    else
    {
      throw new System.Exception("Unrecognized state");

    }
  }

  void Update()
  {
    // Get minion facing the direction of movement
    if (rb2d)
    {

      if (rb2d.velocity.x < float.Epsilon)
      {
        sp.flipX = true;
      }
      else if (rb2d.velocity.x > float.Epsilon)
      {
        sp.flipX = false;
      }

      // Set Running animations
      animator.SetBool("IsMoving", rb2d.velocity != Vector2.zero);
    }

  }

  void FixedUpdate()
  {
    if (currentState == MinionStates.Seeking)
    {
      DoSeek();
    }
    else if (currentState == MinionStates.Wander)
    {
      DoWander();
    }
    else if (currentState == MinionStates.Attack)
    {
      DoAttack();
    }
    else if (currentState == MinionStates.Following)
    {
      DoFollowing();
    }
    else if (currentState == MinionStates.Dead)
    {
      DoDead();
    }

    // Can died from any state
    if (!IsAlive() && currentState != MinionStates.Dead)
    {
      SetState(MinionStates.Dead);
    }

    // Can attack from any state except when dead
    else if (IsTargetInAttackRange())
    {
      if (IsAttackNotOnCooldown() && IsTargetAlive())
      {
        SetState(MinionStates.Attack);
      }
      else
      {
        attackCooldown += Time.fixedDeltaTime;
      }
    }
  }

  void DoWander()
  {
    if (!IsTargetAlive())
    {
      target = FindClosetEnemy();
    }

    if (IsAlly)
    {
      if (IsTargetInRange())
      {
        SetState(MinionStates.Seeking);
      }
      else if (IsPlayerInRange())
      {
        SetState(MinionStates.Wander);
      }
      else if (IsPlayerFar())
      {
        SetState(MinionStates.Following);
      }
    }
    else if (!IsAlly)
    {
      if (IsTargetInRange())
      {
        SetState(MinionStates.Seeking);
      }
      else
      {
        SetState(MinionStates.Wander);
      }
    }
  }

  void DoSeek()
  {
    if (IsAlly)
    {
      if (!IsTargetInRange())
      {
        SetState(MinionStates.Following);
      }
    }
    else if (!IsAlly)
    {
      if (!IsTargetAlive() || !IsTargetInRange())
      {
        target = null;
        minionMovement.target = player;
        SetState(MinionStates.Wander);
      }
    }
  }

  void DoAttack()
  {
    if (IsTargetAlive())
    {
      SetState(MinionStates.Seeking);
    }
    else if (IsAlly && IsPlayerFar())
    {
      SetState(MinionStates.Following);
    }
    else
    {
      SetState(MinionStates.Wander);
    }
  }

  void DoFollowing()
  {
    if (!IsTargetAlive() || !IsTargetInRange())
    {
      target = FindClosetEnemy();
    }

    if (target)
    {
      SetState(MinionStates.Seeking);
    }
    else if (IsPlayerFar())
    {
      // Continue Following
    }
    else if (IsPlayerInRange())
    {
      SetState(MinionStates.Wander);
    }
  }

  void DoDead()
  {
    if (IsAlly) {
      SetState(MinionStates.Following);
    } 
  }

  // Trigger that causes the minion to take damage
  void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.name == "Bullet(Clone)" && !IsAlly)
    {
      DamageHealth(playerSpellDamage);
      Destroy(other);
    }
    else if (other.tag == "BossFireballTrigger" && IsAlly)
    {
      DamageHealth(bossFireballDamage);
      other.GetComponentInParent<BossFireballFlocking>().DeleteFireball();
    }
  }

  public void DamageHealth(float damage)
  {
    animator.SetTrigger("Damaged");
    health -= damage;
  }

  public bool IsAlive()
  {
    return health > 0;
  }

  bool IsTargetAlive()
  {
    if (!target)
    {
      return false;
    }

    bool alive = false;
    if (target.name == "Boss")
    {
      alive = bossStats.IsAlive();
    }
    else if (target.tag == "Minion")
    {
      alive = target.gameObject.GetComponent<Minion>().IsAlive();
    }
    else if (target.name == "Player")
    {
      alive = target.gameObject.GetComponent<PlayerStats>().IsAlive();
    }

    return alive;
  }

  bool IsAttackNotOnCooldown()
  {
    return (attackCooldown >= 1f);
  }

  bool IsTargetInAttackRange()
  {
    if (!target)
    {
      return false;
    }
    return ((target.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(attackRange, 2));
  }

  bool IsTargetInRange()
  {
    if (!target)
    {
      return false;
    }
    return (target.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(detectionRange, 2);
  }

  bool IsPlayerInRange()
  {
    return (player.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(wanderDistance, 2);
  }

  bool IsPlayerFar()
  {
    return (player.transform.position - transform.position).sqrMagnitude >= Mathf.Pow(followDistance, 2);
  }

  // Find closet object for minion to attack
  GameObject FindClosetEnemy()
  {
    // If the minion is on the player side, the target is the boss
    if (IsAlly)
    {
      return (boss.transform.position - transform.position).sqrMagnitude <= Mathf.Pow(detectionRange, 2) ? boss : null;
    }

    // Get all allies within circular range
    Collider2D[] cast = Physics2D.OverlapCircleAll(transform.position, detectionRange, allyLayer);

    GameObject closet = null;
    float distance = Mathf.Infinity;
    Vector3 position = transform.position;

    foreach (Collider2D target in cast)
    {
      // Exclude the current target
      if (this.target != null)
      {
        if (target.transform.position == this.target.transform.position)
        {
          continue;
        }
      }

      Vector3 diff = target.transform.parent.gameObject.transform.position - position;
      float currentDistance = diff.sqrMagnitude;
      if (currentDistance < distance)
      {
        closet = target.transform.parent.gameObject;
        distance = currentDistance;
      }
    }

    // Also check if player is closer
    if (playerStats.IsAlive())
    {
      if ((player.transform.position - transform.position).sqrMagnitude < distance && (player.transform.position - transform.position).sqrMagnitude < Mathf.Pow(detectionRange, 2))
      {
        closet = player;
      }
    }

    return closet;
  }

}
