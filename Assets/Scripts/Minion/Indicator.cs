using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{

    public Sprite sprite;
    private SpriteRenderer spriteR;
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
    }

    public void changeIndicator() {
        spriteR.sprite = sprite;
        spriteR.color = new Color(0.6f, 1f, 0.9f, 1f);
    }
}
