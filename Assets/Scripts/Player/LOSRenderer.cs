using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LOSRenderer : MonoBehaviour
{
    LineRenderer LOS;
    Vector2 mouseTouch, playerPosition;
    public string Tag;

    // Start is called before the first frame update
    void Start()
    {
        this.LOS = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        this.drawLOS();
    }

    void drawLOS()
    {
        this.mouseTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.playerPosition = GameObject.FindWithTag(this.Tag).transform.position;
        this.LOS.SetPosition(0, new Vector3(this.playerPosition.x, this.playerPosition.y, 0.0f));
        this.LOS.SetPosition(1, new Vector3(this.mouseTouch.x, this.mouseTouch.y, 0.0f));
    }
}
