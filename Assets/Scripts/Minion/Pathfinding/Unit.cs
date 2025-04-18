using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Unit : MonoBehaviour
{

  public Transform target;
  private float pathDetectionRange = 50;
  private MinionMovement minionMovement;

  Vector2[] path;
  int targetIndex;

  public LayerMask wallLayer;
  public LayerMask puddleLayer;

  public bool CoroutineActive = false;

  void Start()
  {
    minionMovement = gameObject.GetComponent<MinionMovement>();
  }

  void FixedUpdate()
  {
    if (CoroutineActive) {
      // If the last point of the path is the target, Apply Pursuit Steering Behaviour
      if (path != default(Vector2[]) && targetIndex != default(int) && targetIndex == path.Length - 1)
      {
        minionMovement.Pursuit();
      }

      // Apply Wander Steering Behaviour for obstacle avoidance
      minionMovement.Wander();
    }
  }

  public IEnumerator RefreshPath()
  {
    CoroutineActive = true;
    Vector2 targetPositionOld = (Vector2)target.position + Vector2.up; // ensure != to target.position initially

    while (true)
    {
      if (targetPositionOld != (Vector2)target.position)
      {
        targetPositionOld = (Vector2)target.position;

        path = Pathfinding.RequestPath(transform.position, target.position);
        StopCoroutine("FollowPath");
        StartCoroutine("FollowPath");
      }

      yield return new WaitForSeconds(.25f);
    }
  }

  IEnumerator FollowPath()
  {
    if (path.Length > 0)
    {
      targetIndex = 0;
      Vector2 currentWaypoint = path[0];
      int nextTarget = targetIndex;

      while (true)
      {
        if (Vector2.Distance((Vector2)transform.position, currentWaypoint) <= 0.25f)
        {
          targetIndex++;
          if (targetIndex >= path.Length)
          {
            yield break;
          }

          currentWaypoint = path[targetIndex];
        }

        // Stores the points where the raycast hits a wall
        List<Vector2> hitPoint = new List<Vector2>();

        // Scan 90° in front of the unit for obstacles 
        for (int i = -45; i < 45; i += 1)
        {
          Vector2 direction = Quaternion.Euler(0, 0, i) * (currentWaypoint - (Vector2)transform.position).normalized;
          RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, pathDetectionRange, wallLayer);
          if (hit)
          {
            hitPoint.Add(hit.point);
          }
        }

        float minDistance = Mathf.Infinity;

        // Path smoothing
        for (int i = 0; i < hitPoint.Count; i++)
        {
          for (int j = targetIndex + 1; j < path.Length; j++)
          {
            float distance = Vector2.Distance(path[j], (Vector2)transform.position);
            Vector2 direction = (path[j] - (Vector2)transform.position).normalized;

            // Find the hit point where the distance between the path waypoint and the hit is the smallest
            if (Vector2.Distance(path[j], hitPoint[i]) < minDistance)
            {
              // Double check if something is in the way
              RaycastHit2D obstacleInTheWay = Physics2D.Raycast(transform.position, direction, distance, wallLayer);
              RaycastHit2D puddleInTheWay = Physics2D.Raycast(transform.position, direction, distance, puddleLayer);

              // If the path is clear take the shortcut
              if (!obstacleInTheWay && !puddleInTheWay)
              {
                currentWaypoint = path[j];
                targetIndex = j;
                minDistance = Vector2.Distance(path[j], hitPoint[i]);
              }
            }
          }
        }

        yield return new WaitForSeconds(0.05f);

        if (targetIndex != path.Length - 1)
        {
          minionMovement.Steer(currentWaypoint - (Vector2)transform.position);
        }

        yield return null;

      }

    }
  }

  public void OnDrawGizmos()
  {
    if (path != null)
    {
      for (int i = targetIndex; i < path.Length; i++)
      {
        Gizmos.color = Color.black;

        if (i == targetIndex)
        {
          Gizmos.DrawLine(transform.position, path[i]);
        }
        else
        {
          Gizmos.DrawLine(path[i - 1], path[i]);
        }
      }
    }
  }
}
