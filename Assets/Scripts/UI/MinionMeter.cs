using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionMeter : MonoBehaviour {
    public RectTransform canvas;
    public GameObject skullPrefab;
    public float spacing;
    public float onscreenOffset;
    public float offscreenOffset;
    public float animationRate;

    MinionMeterSkull[] skulls;
    float targetOffset;

    void Start() {
        float initialY = canvas.rect.height * 0.5f + onscreenOffset;
        transform.localPosition = new Vector3(0, initialY, 0);
        targetOffset = onscreenOffset;
    }

    void Update() {
        float targetY = canvas.rect.height * 0.5f + targetOffset;
        float currY = transform.localPosition.y;
        float newY = currY + (targetY - currY) * animationRate * Time.deltaTime;
        transform.localPosition = new Vector3(0, newY, 0);
    }

    public void Setup(int value, int max) {
        skulls = new MinionMeterSkull[max];

        for (int i = 0; i < max; i++) {
            GameObject skull = Instantiate(skullPrefab, transform);
            skulls[i] = skull.GetComponent<MinionMeterSkull>();

            Vector2 position = new Vector2(spacing * i - spacing * (max - 1) * 0.5f, 0);
            skull.GetComponent<RectTransform>().localPosition = position;
        }

        SetValue(value, false);
    }
    
    public void SetValue(int value, bool animate) {
        for (int i = 0; i < skulls.Length; i++) {
            if (i < value) { skulls[i].Show(animate); }
            else { skulls[i].Hide(animate); }
        }
    }
    
    public void Hide() {
        // Called when the player dies or the boss room is entered.
        targetOffset = offscreenOffset;
    }

    public void Show() {
        // Called when the player leaves the boss room.
        targetOffset = onscreenOffset;
    }
}
