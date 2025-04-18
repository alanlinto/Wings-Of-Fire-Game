using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthBar : MonoBehaviour {
    public RectTransform canvas;
    public GameObject background;
    public GameObject bar;
    public GameObject barContainer;

    public float originX;
    public float width;
    public float onscreenOffset;
    public float offscreenOffset;
    public float animationRate;
    
    float percentage;
    float targetOffset;

    void Start() {
        SetPercentage(1);

        float initialY = canvas.rect.height * 0.5f + offscreenOffset;
        transform.localPosition = new Vector3(0, initialY, 0);
        targetOffset = offscreenOffset;
    }

    void Update() {
        float targetY = canvas.rect.height * 0.5f + targetOffset;
        float currY = transform.localPosition.y;
        float newY = currY + (targetY - currY) * animationRate * Time.deltaTime;
        transform.localPosition = new Vector3(0, newY, 0);
    }

    public void SetPercentage(float percentage) {
        bar.transform.localScale = new Vector3(percentage, 1, 1);

        float xPosition = originX - width * (1 - percentage) * 0.5f;  
        bar.transform.localPosition = new Vector3(xPosition, 0, 0);
    }

    public void Show() {
        // Called when the player enters the boss room.
        targetOffset = onscreenOffset;
    }

    public void Hide() {
        // Called when the boss is killed.
        targetOffset = offscreenOffset;
    }
}
