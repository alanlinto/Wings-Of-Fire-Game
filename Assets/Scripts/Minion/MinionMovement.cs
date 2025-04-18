using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionMovement : MonoBehaviour
{
    public float speed;
    public float attackRange;
    public GameObject target;
    //public GameObject wall;
    public Vector3 obstacleCheck;
    private Rigidbody2D rb;
    private Vector2 targetVelocity;
    private Vector2 targetDirection;
    //private Vector3 wallPoint;
    private float circleRadius = 100;
    //private float obstacleCheckLength = 0.3f;

    // time in the future in order for pursuit
    private float futureTime = 5;

    //radius for when to start slowing (could possibly be attributed to the player object instead)
    private float slowingRadius = 5;

    // random angle for minion to start wandering in
    private float randAngle;

    private Vector2 position;

    private float velocityRotation;

    private float raycastDistance = 0.4f;

    public float multiplier;

    public LayerMask minionLayer;
    public LayerMask allyLayer;
    public LayerMask playerLayer;
    public LayerMask wallLayer;


    // Start is called before the first frame update
    void Start()
    {
        randAngle = Random.Range(0, 360) * Mathf.Deg2Rad;
        //print(randAngle * Mathf.Rad2Deg);

        //randAngle = 180;

        rb = GetComponent<Rigidbody2D>();
        //targetDirection = Vector2.right;
        //rb.velocity = Vector2.right;

    }

    // Update is called once per frame
    void Update()
    {
        if (target != null) { 
            position = gameObject.transform.position;
            targetVelocity = target.GetComponent<Rigidbody2D>().velocity;
            velocityRotation = Mathf.Atan2(rb.velocity.normalized.y, rb.velocity.normalized.x);
        }
    }

    private void FixedUpdate()
    {

        //Wander();
        //AvoidMovingCollisions();
        //Steer(targetDirection);

    }

    public void Pursuit()
    {
        if (target == null) { 
            return;
        }
        Vector2 targetPosition = new Vector2(target.transform.position.x + targetVelocity.x * futureTime, target.transform.position.y + targetVelocity.y * futureTime);
        targetDirection = targetPosition - position;

        AvoidObstacles();
        Steer(targetDirection);
    }

    public void Evade()
    {
        Vector2 targetPosition = new Vector2(target.transform.position.x + targetVelocity.x * futureTime, target.transform.position.y + targetVelocity.y * futureTime);
        targetDirection = position - targetPosition;

        Steer(targetDirection);
    }

    public void Wander()
    {
        if (!rb) {return;}

        // establishes imaginary centre circle ahead of the player
        Vector2 circleCentre = rb.velocity.normalized * circleRadius;

        //print(randAngle * Mathf.Rad2Deg);

        // creates point on imaginary circle at the random angle for minion to travel to
        Vector2 circlePoint = new Vector2(Mathf.Cos(randAngle) * circleRadius, Mathf.Sin(randAngle) * circleRadius);

        //Calculates direction to steer to
        targetDirection = circleCentre + circlePoint;

        //print(randAngle * Mathf.Rad2Deg);

        AvoidObstacles();
        AvoidMovingCollisions();
        Steer(targetDirection);

        float wanderAngle = 45 * Mathf.Deg2Rad;

        // changes the angle for the minion to wander
        // (used +-randAngle as the boundary angles but can be changed (e.g. (-90,90)) (in radians)
        randAngle += Random.Range(-wanderAngle, wanderAngle);
    }
    public void AvoidObstacles()
    {
        //float velocityRotation = Mathf.Atan2(rb.velocity.normalized.y, rb.velocity.normalized.x);

        Vector2 offset = new Vector2(Mathf.Cos(velocityRotation + (90 * Mathf.Deg2Rad)), Mathf.Sin(velocityRotation + (90 * Mathf.Deg2Rad))) / 6;

        Vector2 raycastDirection = rb.velocity.normalized;

        RaycastHit2D hit1 = Physics2D.Raycast(position + offset, rb.velocity.normalized, raycastDistance, layerMask: wallLayer);

        RaycastHit2D hit2 = Physics2D.Raycast(position - offset, rb.velocity.normalized, raycastDistance, layerMask: wallLayer);


        Debug.DrawRay(position + offset, rb.velocity.normalized, Color.red);
        Debug.DrawRay(position, rb.velocity.normalized, Color.white);
        Debug.DrawRay(position - offset, rb.velocity.normalized, Color.blue);

        if (hit1.collider != null && hit2.collider != null)
        {
            //print("both hit");
            AvoidObstacles(hit1, hit2, raycastDirection, raycastDirection, 0);
        }
        else if (hit1.collider != null && hit2.collider == null)
        {
            //print("red hit");
            targetDirection = position - hit1.point;
        }
        else if (hit1.collider == null && hit2.collider != null)
        {
            //print("blue hit");
            targetDirection = position - hit2.point;
        }

        randAngle = velocityRotation;

    }

    public void AvoidObstacles(RaycastHit2D hit1, RaycastHit2D hit2, Vector2 raycastDirectionLeft, Vector2 raycastDirectionRight, int counter)
    {
        Vector2 offset = new Vector2(Mathf.Cos(velocityRotation + (90 * Mathf.Deg2Rad)), Mathf.Sin(velocityRotation + (90 * Mathf.Deg2Rad))) / 6;
        if (counter != 4)
        {

            if (hit1.collider != null && hit2.collider != null)
            {
                //print("both hit");

                float raycastAngleLeft = Mathf.Atan2(raycastDirectionLeft.y, raycastDirectionLeft.x) + (45 * Mathf.Deg2Rad);
                float raycastAngleRight = Mathf.Atan2(raycastDirectionRight.y, raycastDirectionRight.x) - (45 * Mathf.Deg2Rad);

                raycastDirectionLeft = new Vector2(Mathf.Cos(raycastAngleLeft), Mathf.Sin(raycastAngleLeft));
                raycastDirectionRight = new Vector2(Mathf.Cos(raycastAngleRight), Mathf.Sin(raycastAngleRight));

                RaycastHit2D newRayLeft = Physics2D.Raycast(position + offset, raycastDirectionLeft, 1, layerMask: wallLayer);
                RaycastHit2D newRayRight = Physics2D.Raycast(position - offset, raycastDirectionRight, 1, layerMask: wallLayer);
                Debug.DrawRay(position + offset, raycastDirectionLeft, Color.magenta);
                Debug.DrawRay(position - offset, raycastDirectionRight, Color.cyan);

                AvoidObstacles(newRayLeft, newRayRight, raycastDirectionLeft, raycastDirectionRight, ++counter);
            }
            else if (hit1.collider != null && hit2.collider == null || hit1.collider == null && hit2.collider == null)
            {
                //Debug.DrawRay(position - offset, raycastDirectionRight, Color.cyan);
                
                targetDirection = raycastDirectionRight - position;


            }
            else if (hit1.collider == null && hit2.collider != null)
            {
                //Debug.DrawRay(position - offset, raycastDirectionLeft, Color.cyan);

                targetDirection = raycastDirectionLeft - position;
            }
        }

    }

    public void AvoidMovingCollisions()
    {
        float localRadius = 3;
        // Check which units to collider based on current status
        LayerMask layerToUse = GetComponent<Minion>().IsAlly ? allyLayer : minionLayer;
        
        // Get units that risk collision
        Collider2D[] minionThreats = Physics2D.OverlapCircleAll(transform.position, localRadius, layerToUse);
        Collider2D[] playerThreat = Physics2D.OverlapCircleAll(transform.position, localRadius, playerLayer);

        Collider2D[] collisionThreats = new Collider2D[minionThreats.Length + (GetComponent<Minion>().IsAlly ? playerThreat.Length : 0)];

        minionThreats.CopyTo(collisionThreats, 0);
        if (GetComponent<Minion>().IsAlly) {
            playerThreat.CopyTo(collisionThreats, minionThreats.Length);
        }

        if(collisionThreats.Length > 1)
        {
            // print("yes");
            foreach(Collider2D collider in collisionThreats)
            {
                if(collider.gameObject != gameObject)
                {
                    if (collider.gameObject.transform.parent.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Static) {return;}
                    Vector2 colliderVel = collider.gameObject.transform.parent.GetComponent<Rigidbody2D>().velocity;

                    Vector2 colliderPos = collider.gameObject.transform.position;


                    if (colliderVel.normalized != rb.velocity.normalized)
                    {
                        Vector2 intersectionPoint;

                        intersectionPoint.x = ((position.x * rb.velocity.y - position.y * rb.velocity.x) * (colliderPos.x - colliderVel.x) - (position.x - rb.velocity.x) * (colliderPos.x * colliderVel.y - colliderPos.y * colliderVel.x))
                                                / ((position.x - rb.velocity.x) * (colliderPos.y - colliderVel.y) - (position.y - rb.velocity.y) * (colliderPos.x - colliderVel.x));

                        intersectionPoint.y = ((position.x * rb.velocity.y - position.y * rb.velocity.x) * (colliderPos.y - colliderVel.y) - (position.y - rb.velocity.y) * (colliderPos.x * colliderVel.y - colliderPos.y * colliderVel.x))
                                                / ((position.x - rb.velocity.x) * (colliderPos.y - colliderVel.y) - (position.y - rb.velocity.y) * (colliderPos.x - colliderVel.x));

                        if(Vector2.Distance(position, intersectionPoint) <= localRadius)
                        {
                            
                            Debug.DrawLine(position, intersectionPoint);
                            // print("intersection"+ intersectionPoint);
                            targetDirection = new Vector2(Mathf.Cos(velocityRotation + (10 * Mathf.Deg2Rad)), Mathf.Sin(velocityRotation + (10 * Mathf.Deg2Rad)));

                            randAngle = velocityRotation;

                        }
                    }

                }
            }
        }
    }


    public void Steer(Vector2 targetDirection)
    {
        if (target == null) {
            return;
        }
        
        Vector2 desiredVelocity = targetDirection.normalized * speed * (GetComponent<Minion>().currentState == MinionStates.Following ? multiplier : 1);

        // Uses an arrive to slow down minion when getting close to target
        float distance = Vector2.Distance(gameObject.transform.position, target.transform.position);
        
        if (GetComponent<Minion>().currentState == MinionStates.Seeking && distance < slowingRadius)
        {
            desiredVelocity *= (distance / slowingRadius);

            // Will make the minion stop when it is in its attack range
            if (distance < attackRange)
            {
                desiredVelocity = Vector2.zero;
            }
        }

        Vector2 steering = desiredVelocity - rb.velocity;

        rb.AddForce(steering);
    }
}