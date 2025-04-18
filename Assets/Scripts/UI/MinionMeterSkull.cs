using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionMeterSkull : MonoBehaviour {
    public float enabledOpacity;
    public float disabledOpacity;
    public float opacityAnimationRate;
    public float enabledScale;
    public float disabledScale;
    public float scaleAnimationRate;

    float targetOpacity;
    float opacity;
    float targetScale;
    float scale;

    Image image;

    void Start() {
        image = this.GetComponent<Image>();
    }

    void Update() {
        opacity += (targetOpacity - opacity) * opacityAnimationRate * Time.deltaTime;
        image.color = new Color(1, 1, 1, opacity);

        scale += (targetScale - scale) * scaleAnimationRate * Time.deltaTime;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void Show(bool animate) {
        targetOpacity = enabledOpacity;
        targetScale = enabledScale;
        if (!animate) {
            opacity = enabledOpacity;
            scale = enabledScale;
        }
    }

    public void Hide(bool animate) {
        targetOpacity = disabledOpacity;
        targetScale = disabledScale;
        if (!animate) {
            opacity = disabledOpacity;
            scale = disabledScale;
        }
    }
}
