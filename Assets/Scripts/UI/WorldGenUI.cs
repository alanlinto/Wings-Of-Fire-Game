using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldGenUI : MonoBehaviour {
    public RectTransform canvas;
    public RawImage imageUI;
    public GameObject panel;
    public Text seedText;
    public Text titleText;
    public float animationDuration;

    int width;
    int height;
    Texture2D texture;

    bool fadeOut;
    float opacity;

    new CameraFollow camera;

    void Start() {
        panel.SetActive(true);
        titleText.gameObject.SetActive(true);
        seedText.gameObject.SetActive(true);
        imageUI.gameObject.SetActive(true);

        RectTransform transform = this.GetComponent<RectTransform>();
        float scale = canvas.rect.height / transform.rect.height;
        transform.localScale = new Vector3(scale, scale, 1);

        fadeOut = false;
    }

    void FixedUpdate() {
        if (fadeOut) {
            opacity -= Time.fixedDeltaTime / animationDuration;
            this.SetOpacity(opacity < 0 ? 0 : opacity);

            if (opacity < 0) {
                panel.SetActive(false);
                titleText.gameObject.SetActive(false);
                seedText.gameObject.SetActive(false);
                imageUI.gameObject.SetActive(false);
                fadeOut = false;
                camera.OnWorldGenDone();
            }
        }
    }

    public void Prep(int width, int height) {
        texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        imageUI.texture = texture;

        this.width = width;
        this.height = height;

        this.fadeOut = false;
        this.opacity = 1;
        this.SetOpacity(opacity);

        seedText.text = "Seed: " + MainMenuController.seed;
    }

    public void SubmitImage(long[] walls, long[] puddles) {
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                long wallColor = walls[y * width + x];
                long puddleColor = puddles[y * width + x];

                int wallR = (int) ((wallColor & 0xff0000) >> 16);
                int wallG = (int) ((wallColor & 0xff00) >> 8);
                int wallB = (int) (wallColor & 0xff);
                int puddleR = (int) ((puddleColor & 0xff0000) >> 16);
                int puddleG = (int) ((puddleColor & 0xff00) >> 8);
                int puddleB = (int) (puddleColor & 0xff);

                Color color = new Color(
                    Mathf.Min(wallR, puddleR) / 255f, 
                    Mathf.Min(wallG, puddleG) / 255f,
                    Mathf.Min(wallB, puddleB) / 255f,
                    1
                );
                
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
    }

    public void Hide() {
        fadeOut = true;
    }

    void SetOpacity(float opacity) {
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, opacity * opacity);
        imageUI.color = new Color(1f, 1f, 1f, opacity);
        seedText.color = new Color(1f, 1f, 1f, opacity * opacity);
        titleText.color = new Color(1f, 1f, 1f, opacity * opacity);
    }

    public void ProvideCamera(CameraFollow camera) {
        this.camera = camera;
    }
}
