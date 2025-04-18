using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossFireballFlocking : MonoBehaviour {
    public GameObject fireballsParent;
    public GameObject sprite;

    public Vector2 initialVelocity;
    public float despawnSeconds;
    public float flockmateRadius;
    public float cohesionStrength;
    public float separationStrength;
    public float alignmentStrength;
    public float maxSpeed;
    public LayerMask wallLayer;
    public float wallSight;
    public float wallRepelStrength;
    public float spriteAngleOffset;

    // Fireball Automatic Restart Mechanism™ (FARM).
    public float FARMIdleDuration;
    public float FARMSlowVelocity;
    public float FARMTargetVelocity;
    public float FARMStrength;
    float FARMTimer;
    bool FARMActive;
    float FARMAngle;


    // "new" keyword to override deprecated "rigidbody" field in base class.
    new Rigidbody2D rigidbody;

    float despawnTimer;

    void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
        despawnTimer = despawnSeconds;

        rigidbody.velocity = initialVelocity;
    }

    void FixedUpdate() {
        despawnTimer -= Time.fixedDeltaTime;
        if (despawnTimer <= 0) {
            GameObject.Destroy(gameObject);
        }

        Vector2 edgeForce = GetEdgeForce();
        if (edgeForce.sqrMagnitude > 0.01) {
            rigidbody.AddForce(edgeForce * wallRepelStrength);
            FARMActive = false;
            FARMTimer = FARMIdleDuration;
        }
        else {
            BossFireballFlocking[] flockmates = GetLocalFlockmates();
            rigidbody.AddForce(GetCohesionForce(flockmates) * cohesionStrength);
            rigidbody.AddForce(GetSeparationForce(flockmates) * separationStrength);
            rigidbody.AddForce(GetAlignmentForce(flockmates) * alignmentStrength);
            rigidbody.AddForce(GetFARMForce() * FARMStrength);
        }


        if (rigidbody.velocity.magnitude > maxSpeed) {
            rigidbody.velocity = rigidbody.velocity.normalized * maxSpeed;
        }

        float angle = Mathf.Atan2(rigidbody.velocity.y, rigidbody.velocity.x);
        sprite.transform.localRotation = Quaternion.Euler(
            0, 0, angle / Mathf.PI * 180 + spriteAngleOffset
        );
    }

    BossFireballFlocking[] GetLocalFlockmates() {
        return fireballsParent.GetComponentsInChildren<BossFireballFlocking>()
            .Where(x => {
                float dist = (this.GetPosition() - x.GetPosition()).magnitude;
                return dist > 0 && dist < flockmateRadius;
            })
            .ToArray();
    }

    Vector2 GetCohesionForce(BossFireballFlocking[] flockmates) {
        if (flockmates.Length < 1) { return Vector2.zero; }

        Vector2 positionSum = Vector2.zero;
        foreach (var flockmate in flockmates) {
            positionSum += flockmate.GetPosition();
        }
        Vector2 averagePosition = positionSum / flockmates.Length;
        Vector2 desiredVelocity = averagePosition - this.GetPosition();
        Vector2 force = (desiredVelocity - this.GetVelocity()).normalized;
        return force;
    }

    Vector2 GetSeparationForce(BossFireballFlocking[] flockmates) {
        if (flockmates.Length < 1) { return Vector2.zero; }

        Vector2 repulsion = Vector2.zero;
        foreach (var flockmate in flockmates) {
            Vector2 flockmateDirection = flockmate.GetPosition() - this.GetPosition();
            float distance = flockmateDirection.magnitude;
            repulsion += -flockmateDirection.normalized / distance;
        }
        return repulsion;
    }

    Vector2 GetAlignmentForce(BossFireballFlocking[] flockmates) {
        if (flockmates.Length < 1) { return Vector2.zero; }

        Vector2 velocitySum = Vector2.zero;
        foreach (var flockmate in flockmates) {
            // Normalize so each flockmate has equal sway over the direction.
            velocitySum += flockmate.GetVelocity().normalized;
        }
        Vector2 averageDirection = velocitySum.normalized * this.GetVelocity().magnitude;
        Vector2 force = (averageDirection - this.GetVelocity()).normalized;
        return force;
    }

    Vector2 GetEdgeForce() {
        Vector2 force = Vector2.zero;
        if (Physics2D.Raycast(transform.position, Vector2.up, wallSight, wallLayer)) {
            force += Vector2.down;
        }
        if (Physics2D.Raycast(transform.position, Vector2.down, wallSight, wallLayer)) {
            force += Vector2.up;
        }
        if (Physics2D.Raycast(transform.position, Vector2.left, wallSight, wallLayer)) {
            force += Vector2.right;
        }
        if (Physics2D.Raycast(transform.position, Vector2.right, wallSight, wallLayer)) {
            force += Vector2.left;
        }
        return force;
    }

    public Vector2 GetFARMForce() {
        float currSpeed = rigidbody.velocity.magnitude;
        if (FARMActive) {
            if (currSpeed > FARMTargetVelocity) {
                FARMActive = false;
                FARMTimer = FARMIdleDuration;
            } 
            else {
                return new Vector2(Mathf.Cos(FARMAngle), Mathf.Sin(FARMAngle));
            }
        }
        else {
            if (currSpeed > FARMSlowVelocity) {
                FARMTimer = FARMIdleDuration;
            }
            else {
                FARMTimer -= Time.fixedDeltaTime;
                if (FARMTimer < 0) {
                    FARMActive = true;
                    FARMAngle = Random.Range(0, Mathf.PI * 2);
                }
            }
        }    
        return Vector2.zero;    
    }

    public Vector2 GetPosition() {
        return this.transform.position;
    }

    public Vector2 GetVelocity() {
        if (this.rigidbody == null) { return Vector2.zero; }
        return this.rigidbody.velocity.normalized;
    }

    public void DeleteFireball() {
        Destroy(this.gameObject);
    }
}
