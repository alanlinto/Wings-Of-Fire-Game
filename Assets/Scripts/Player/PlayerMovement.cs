using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [SerializeField]
    float speed;
    // "new" keyword to override deprecated "rigidbody" field in base class.
    new Rigidbody2D rigidbody;

    Animator animator;
    //SpriteRenderer sprite;
    bool faceRight;

    PlayerStats stats;

    void Start() {
        faceRight = true;
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();
        //sprite = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate() {
        if (stats.IsAlive()) {
            // Use the x-component of the input to flip the sprite if necessary.
            float x = Input.GetAxis("Horizontal");
            /*if (x > 0) { sprite.flipX = false; }
            if (x < 0) { sprite.flipX = true; }*/

            if (x > 0 && !this.faceRight)
            {
                Flip();
            }
            if(x < 0 && this.faceRight)
            {
                Flip();
            }

            // Combine it with the y-component to get the movement force. Restrict
            // the vector to a maximum length of 1 (so going diagonal isn't faster). 
            float y = Input.GetAxis("Vertical");
            Vector2 movement = new Vector2(x, y);
            
            if (movement.magnitude > 1) {
                movement = movement.normalized;
            }

            // Apply the movement force.
            rigidbody.AddForce(movement * speed);

            // Do the running animation if the player is moving.
            animator.SetBool("WalkingUp", y > 0);
        }
    }

    void Flip()
    {
        //For Flipping the Player sprite based on it's Y axis
        transform.Rotate(0.0f, 180.0f, 0.0f);

        this.faceRight = !(this.faceRight);
    }
}
