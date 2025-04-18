using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AreaBasedCleanup {
    public static void Run(long[] array, int width, int height, long color, 
        long fillColor, int minArea, bool ignoreEdgeAdjacent) {
        
        bool[] explored = new bool[width * height];

        // Start by considering every non-wall/puddle tile to be explored.
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                int i = y * width + x;
                explored[i] = array[i] != color;
            }
        }

        // For every unexplored node (wall/puddle), mark it as explored, and
        // "process the blob" ("blobs" are groups of connected tiles).
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                int i = y * width + x;
                if (explored[i]) { continue; }

                explored[i] = true;
                ProcessBlob(x, y, array, width, height, fillColor, minArea, ignoreEdgeAdjacent, explored);
            }
        }
    }

    static void ProcessBlob(int startX, int startY, long[] array, int width, 
        int height, long fillColor, int minArea, bool ignoreEdgeAdjacent, 
        bool[] explored) {
        
        // Keep a list of all tiles that are connected to this one.
        List<(int x, int y)> thisBlob = new();
        thisBlob.Add((startX, startY));

        // Track if the blob touches the edge (blobs touching the edges are 
        // ignored if ignoreEdgeAdjacent is true).
        bool touchesEdge = false;

        for (int n = 0; n < thisBlob.Count; n++) {
            // Guaranteed to be a new cell that hasn't had it's neighbours
            // checked yet, because anything added to thisBlob is immediately
            // marked as explored, and nothing is ever added unless it was 
            // previously unexplored.
            (int x, int y) = thisBlob[n];
            
            // This tile was already added to the blob, so if it touches a wall,
            // the blob does too.
            if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                touchesEdge = true;
            }

            // Add any unexplored neighbours to this blob (remember that other 
            // colors are automatically considered explored) assuming they're 
            // not outside the array's bounds.
            AddToBlobIfAppropriate(x - 1, y, thisBlob, width, height, explored);
            AddToBlobIfAppropriate(x, y - 1, thisBlob, width, height, explored);
            AddToBlobIfAppropriate(x + 1, y, thisBlob, width, height, explored);
            AddToBlobIfAppropriate(x, y + 1, thisBlob, width, height, explored);
        }

        if (touchesEdge && ignoreEdgeAdjacent) { return; }

        // Ignore the blob if it's above the threshold size. Note that because
        // it has been marked as explored now, it won't be detected again.
        if (thisBlob.Count >= minArea) { return; }

        // Remove the blob by filling it with another color.
        foreach (var coords in thisBlob) {
            int i = coords.y * width + coords.x;
            array[i] = fillColor;
        }
    }

    static void AddToBlobIfAppropriate(int x, int y, 
        List<(int x, int y)> thisBlob, int width, int height, bool[] explored) {
        
        // Don't attempt to add tiles outside the map to the blob.
        if (x < 0 || x >= width || y < 0 || y >= height) { return; }

        int i = y * width + x;

        // Add the node, only if it hasn't been explored yet.
        if (!explored[i]) {
            thisBlob.Add((x, y));
            explored[i] = true;
        }
    }
}