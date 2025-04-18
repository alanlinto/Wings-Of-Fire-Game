using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowLeader : MonoBehaviour
{
    // Start is called before the first frame update
    //public 
    public Transform player;
    float velocity, distance;
    Rigidbody2D rb;
    Vector3 move,temp;
    bool flag;

    void Start()
    {
        this.velocity = 0.0f;
        this.rb = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(flag)
        {
            this.calc_direction();
        }
    }

    void FixedUpdate()
    {
        if(this.temp.x != transform.position.x || this.temp.y != transform.position.y)
        {
            flag = true;
        }

        if(flag)
        {
            this.add_force();
        }
    }

    void calc_direction()
    {
        Vector3 dir = player.position - transform.position;
        //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg
        dir.Normalize();
        this.move = dir;
        this.distance = Vector3.Distance(this.transform.position, this.player.position);
        //Debug.Log(distance);
        if ((int)distance == 1)
        {
            this.velocity = 0.0f;
        }
        else
        {
            this.velocity = 2.5f;
        }
    }

    void add_force()
    {
        this.temp = transform.position + (this.move * this.velocity * Time.deltaTime);
        rb.MovePosition(this.temp);
    }
}
