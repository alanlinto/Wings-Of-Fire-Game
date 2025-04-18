using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventTextFading : MonoBehaviour {
    public float fadeDelay;
    public float fadeDuration;

    Image image;
    float opacity;
    float time;
    bool running;

    void Start() {
        image = this.GetComponent<Image>();
        image.color = new Color(1, 1, 1, 0);

        opacity = 0;
        time = 0;
        running = false;
    }

    void Update() {
        if (running) {
            time += Time.deltaTime;
            if (time < fadeDelay) {
                opacity = 0;
            }
            else if (time < fadeDelay + fadeDuration) {
                opacity = (time - fadeDelay) / fadeDuration;
            }
            else {
                opacity = 1;
            }
        }
        
        image.color = new Color(1, 1, 1, opacity);
    }

    public void Show() {
        running = true;
    }
}
