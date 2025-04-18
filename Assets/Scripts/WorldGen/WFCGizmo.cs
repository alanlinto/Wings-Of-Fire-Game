using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCGizmo : MonoBehaviour {
    public int width;
    public int height;
    public int n;
    public int seed;
    public int symmetry;
    public Texture2D sample;
    public float cubeSize;
    public int steps;

    long[] output;

    void Start() {
        WaveFunctionCollapse wfc = new WaveFunctionCollapse(
            sample, width, height, n, false, true, symmetry, 
            WaveFunctionCollapse.Heuristic.Entropy
        );
        bool success = wfc.Run(seed, steps);
        if (success) {
            output = wfc.Save();
        }
    }

    void OnDrawGizmos() {
        if (output != null) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int i = y * width + x;
                    long argb = output[i];

                    int r = (int) ((argb & 0xff0000) >> 16);
                    int g = (int) ((argb & 0xff00) >> 8);
                    int b = (int) (argb & 0xff);
                    
                    Gizmos.color = new Color(r / 255f, g / 255f, b / 255f, 1f);
                    Gizmos.DrawCube(new Vector3(x, y, 0) * cubeSize, new Vector3(cubeSize, cubeSize, cubeSize));
                }
            }
        }
    }
}
