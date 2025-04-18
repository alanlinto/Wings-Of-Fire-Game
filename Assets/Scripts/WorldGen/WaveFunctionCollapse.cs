// Wave function collapse algorithm adapted from work by Maxim Gumin (MIT License).
// Copyright (c) 2016 Maxim Gumin
// (https://github.com/mxgmn/WaveFunctionCollapse)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse {
    public enum Heuristic { Entropy, MRV, Scanline };

    protected bool periodic;
    Heuristic heuristic;

    /// <summary> MX and MY are the output width and height. </summary>
    protected int MX, MY;

    /// <summary> The N value (e.g. 3x3 patterns). </summary>
    protected int N;

    /// <summary> Number of unique patterns? </summary>
    protected int T;

    /// <summary> All unique patterns detected in image (array of color indices). </summary>
    List<byte[]> patterns;

    /// <summary> The ARGB value for each color referred to in the patterns array. </summary>
    List<long> colors;

    /// <summary> How many times each unique pattern is present. </summary>
    protected double[] weights;

    /// <summary> 
    /// Given a direction (first index) and pattern index (second index), which 
    /// other patterns can be placed next to it? The values are pattern indices.
    /// </summary>
    protected int[][][] propagator;
    
    /// <summary>
    /// The first dimension is for every output pixel. Second dimension is each
    /// unique pattern. Every value starts as true.
    /// </summary>
    protected bool[][] wave;

    /// <summary>
    /// The first dimension is for every output pixel. Second dimension is each
    /// unique pattern. Third dimension is each direction.
    /// </summary>
    int[][][] compatible;

    /// <summary>
    /// Has an entry for each unique pattern. Only ever used during Observe(). 
    /// Holds the number of times each valid pattern for a certain pixel in the 
    /// output was present in the sample texture.
    /// </summary>
    double[] distribution;
    
    /// <summary>
    /// Has an entry for each unique pattern.
    /// </summary>
    double[] weightLogWeights;

    /// <summary>
    /// Has an entry for each output pixel. Seems to be the output array when
    /// the algorithm has finished running.
    /// </summary>
    protected int[] observed;

    /// <summary>
    /// The total number of patterns including duplicates.
    /// </summary>
    double sumOfWeights, sumOfWeightLogWeights, startingEntropy;

    
    /// <summary>
    /// Has an entry for each output pixel.
    /// </summary>
    protected int[] sumsOfOnes;
    
    /// <summary>
    /// Has an entry for each output pixel.
    /// </summary>
    protected double[] sumsOfWeights, sumsOfWeightLogWeights, entropies;


    (int, int)[] stack;
    int stacksize, observedSoFar;

    // Directions.
    protected static int[] dx = { -1, 0, 1, 0 };
    protected static int[] dy = { 0, 1, 0, -1 };

    /// <summary> Index of the opposite direction. </summary>
    static int[] opposite = { 2, 3, 0, 1 };

    /// <summary> The RNG to use during "continue" mode. </summary>
    System.Random continueRandom;

    public WaveFunctionCollapse(
        Texture2D texture, int width, int height, int n, bool periodic, 
        bool periodicInput, int symmetry, Heuristic heuristic
    ) {
        // =======================
        // Constructor in Model.cd
        // =======================
        MX = width;
        MY = height;
        this.N = n;
        this.periodic = periodic;
        this.heuristic = heuristic;

        // ==================================
        // Constructor in OverlappingModel.cs
        // ==================================

        // Import the image as a 1D array of integers (ARBG)
        int SY = texture.height;
        int SX = texture.width;
        long[] bitmap = texture
            .GetPixels32(0)
            .Select(x => {
                return ((long) x.a) * 256 * 256 * 256 + 
                    ((long) x.r) * 256 * 256 + 
                    ((long) x.g) * 256 + 
                    ((long) x.b);
            })
            .ToArray();
   
        // Count distinct colors and create 1D image array with color indices.
        byte[] sample = new byte[bitmap.Length];
        colors = new List<long>();
        for (int i = 0; i < sample.Length; i++) {
            long color = bitmap[i];
            int k = 0;
            for (; k < colors.Count; k++) {
                if (colors[k] == color) { break; }
            }
            if (k == colors.Count) { colors.Add(color); }
            sample[i] = (byte) k;
        }

        // Returns one pattern (N*N as a 1-dimensional array).
        static byte[] pattern(Func<int, int, byte> f, int N)
        {
            byte[] result = new byte[N * N];
            for (int y = 0; y < N; y++) {
                for (int x = 0; x < N; x++) {
                    result[x + y * N] = f(x, y);
                }
            }
            return result;
        };

        static byte[] rotate(byte[] p, int N) => pattern((x, y) => p[N - 1 - y + x * N], N);
        static byte[] reflect(byte[] p, int N) => pattern((x, y) => p[N - 1 - x + y * N], N);

        static long hash(byte[] p, int C)
        {
            long result = 0, power = 1;
            for (int i = 0; i < p.Length; i++)
            {
                result += p[p.Length - 1 - i] * power;
                power *= C;
            }
            return result;
        };

        // The colors indices array (3x3 if n=3) for each pattern.
        patterns = new();

        // Maps a pattern hash to its index in the weight list.
        Dictionary<long, int> patternIndices = new();

        // How many times each identical pattern exists.
        List<double> weightList = new();

        int C = colors.Count;
        int xmax = periodicInput ? SX : SX - N + 1;
        int ymax = periodicInput ? SY : SY - N + 1;
        for (int y = 0; y < ymax; y++) {
            for (int x = 0; x < xmax; x++) {
                // Create an array with every possible symmetry, rotation, etc. 
                byte[][] ps = new byte[8][];
                ps[0] = pattern((dx, dy) => sample[(x + dx) % SX + (y + dy) % SY * SX], N);
                ps[1] = reflect(ps[0], N);
                ps[2] = rotate(ps[0], N);
                ps[3] = reflect(ps[2], N);
                ps[4] = rotate(ps[2], N);
                ps[5] = reflect(ps[4], N);
                ps[6] = rotate(ps[4], N);
                ps[7] = reflect(ps[6], N);

                // Add as many symmetrical patterns as requested based on the constructors params.
                for (int k = 0; k < symmetry; k++) {
                    byte[] p = ps[k];
                    long h = hash(p, C);
                    if (patternIndices.TryGetValue(h, out int index)) {
                        weightList[index] = weightList[index] + 1;
                    }
                    else {
                        patternIndices.Add(h, weightList.Count);
                        weightList.Add(1.0);
                        patterns.Add(p);
                    }
                }
            }
        }

        weights = weightList.ToArray();
        T = weights.Length;

        // Magic??
        static bool agrees(byte[] p1, byte[] p2, int dx, int dy, int N)
        {
            int xmin = dx < 0 ? 0 : dx;
            int xmax = dx < 0 ? dx + N : N; 
            int ymin = dy < 0 ? 0 : dy;
            int ymax = dy < 0 ? dy + N : N;

            for (int y = ymin; y < ymax; y++) {
                for (int x = xmin; x < xmax; x++) {
                    if (p1[x + N * y] != p2[x - dx + N * (y - dy)]) {
                        return false;
                    }
                }
            }
            return true;
        };

        propagator = new int[4][][];

        // For each adjacent...
        for (int d = 0; d < 4; d++) {
            propagator[d] = new int[T][];

            // For each unique pattern...
            for (int t = 0; t < T; t++) {
                List<int> list = new();

                // For each other unique pattern...
                for (int t2 = 0; t2 < T; t2++) {
                    if (agrees(patterns[t], patterns[t2], dx[d], dy[d], N)) {
                        list.Add(t2);
                    }
                }

                // Copy all patterns that agree with this one (for this 
                // direction) to the propagator.
                propagator[d][t] = new int[list.Count];
                for (int c = 0; c < list.Count; c++) {
                    propagator[d][t][c] = list[c];
                }
            }
        }
    }

    void Init() {
        wave = new bool[MX * MY][];
        compatible = new int[wave.Length][][];

        // For every output pixel...
        for (int i = 0; i < wave.Length; i++) {
            wave[i] = new bool[T];
            compatible[i] = new int[T][];
            for (int t = 0; t < T; t++) { 
                compatible[i][t] = new int[4];
            }
        }

        distribution = new double[T];
        observed = new int[MX * MY];

        weightLogWeights = new double[T];
        sumOfWeights = 0;
        sumOfWeightLogWeights = 0;

        // For each unique pattern...
        for (int t = 0; t < T; t++) {
            weightLogWeights[t] = weights[t] * Math.Log(weights[t]);
            sumOfWeights += weights[t];
            sumOfWeightLogWeights += weightLogWeights[t];
        }

        startingEntropy = Math.Log(sumOfWeights) - sumOfWeightLogWeights / sumOfWeights;

        sumsOfOnes = new int[MX * MY];
        sumsOfWeights = new double[MX * MY];
        sumsOfWeightLogWeights = new double[MX * MY];
        entropies = new double[MX * MY];

        stack = new (int, int)[wave.Length * T];
        stacksize = 0;
    }

    void Clear() {
        for (int i = 0; i < wave.Length; i++) {
            for (int t = 0; t < T; t++) {
                wave[i][t] = true;
                for (int d = 0; d < 4; d++) {
                    compatible[i][t][d] = propagator[opposite[d]][t].Length;
                }
            }

            sumsOfOnes[i] = weights.Length;
            sumsOfWeights[i] = sumOfWeights;
            sumsOfWeightLogWeights[i] = sumOfWeightLogWeights;
            entropies[i] = startingEntropy;
            observed[i] = -1;
        }
        
        observedSoFar = 0;
    }

    public bool Run(int seed, int limit) {
        if (wave == null) {
            Init();
        }

        Clear();
        System.Random random = new(seed);

        for (int l = 0; l < limit || limit < 0; l++) {
            // Get the next pixel we should decide for (based on lowest[?] entropy).
            int node = NextUnobservedNode(random);

            // If there is a next pixel to expand...
            if (node >= 0) {
                Observe(node, random);
                bool success = Propagate();

                // If there's a contradiction.
                if (!success) {
                    return false;
                }
            }
            else {
                // If we're done.
                
                // For each output pixel, find the first pattern still true in
                // the wave table and put that pattern's index in the observed
                // array for this pixel.
                for (int i = 0; i < wave.Length; i++) {
                    for (int t = 0; t < T; t++) {
                        if (wave[i][t]) { 
                            observed[i] = t; break; 
                        }
                    }
                }
                return true;
            }
        }

        return true;
    }

    public bool Start(int seed, int iterations) {
        if (wave == null) {
            Init();
        }

        Clear();
        continueRandom = new(seed);

        return Continue(iterations);
    }
    
    public bool Continue(int iterations) {
        for (int l = 0; l < iterations; l++) {
            // Get the next pixel we should decide for (based on lowest[?] entropy).
            int node = NextUnobservedNode(continueRandom);

            // If there is a next pixel to expand...
            if (node >= 0) {
                Observe(node, continueRandom);
                bool success = Propagate();

                // If there's a contradiction.
                if (!success) {
                    throw new Exception(
                        "This seed caused a contradiction in the WFC " + 
                        "algorithm, please try another."
                    );
                }
            }
            else {
                // If we're done.
                
                // For each output pixel, find the first pattern still true in
                // the wave table and put that pattern's index in the observed
                // array for this pixel.
                for (int i = 0; i < wave.Length; i++) {
                    for (int t = 0; t < T; t++) {
                        if (wave[i][t]) { 
                            observed[i] = t; break; 
                        }
                    }
                }
                return true;
            }
        }
        return false;
    }

    int NextUnobservedNode(System.Random random) {
        if (heuristic == Heuristic.Scanline) {
            for (int i = observedSoFar; i < wave.Length; i++) {
                if (!periodic && (i % MX + N > MX || i / MX + N > MY)) {
                    continue;
                }
                if (sumsOfOnes[i] > 1)
                {
                    observedSoFar = i + 1;
                    return i;
                }
            }
            return -1;
        }

        double min = 1E+4;
        int argmin = -1;
        for (int i = 0; i < wave.Length; i++) {
            if (!periodic && (i % MX + N > MX || i / MX + N > MY)) {
                continue;
            }
            int remainingValues = sumsOfOnes[i];
            double entropy = heuristic == Heuristic.Entropy ? entropies[i] : remainingValues;
            if (remainingValues > 1 && entropy <= min)
            {
                double noise = 1E-6 * random.NextDouble();
                if (entropy + noise < min)
                {
                    min = entropy + noise;
                    argmin = i;
                }
            }
        }
        return argmin;
    }

    void Observe(int node, System.Random random) {
        bool[] w = wave[node];
        for (int t = 0; t < T; t++) {
            distribution[t] = w[t] ? weights[t] : 0.0;
        }

        // Pick a random eligible pattern based on the distribution (how 
        // frequent it was in the sample image).
        int r = RandomFromWeights(distribution, random.NextDouble());

        for (int t = 0; t < T; t++) {
            if (w[t] != (t == r)) {
                // Set every other pattern choice for this pixel to false, now
                // that we've selected one.
                Ban(node, t);
            }
        }
    }

    void Ban(int i, int t) {
        // Marks this choice as invalid for this pixel.

        wave[i][t] = false;

        int[] comp = compatible[i][t];
        for (int d = 0; d < 4; d++) {
            comp[d] = 0;
        }

        stack[stacksize] = (i, t);
        stacksize++;

        sumsOfOnes[i] -= 1;
        sumsOfWeights[i] -= weights[t];
        sumsOfWeightLogWeights[i] -= weightLogWeights[t];

        double sum = sumsOfWeights[i];
        entropies[i] = Math.Log(sum) - sumsOfWeightLogWeights[i] / sum;
    }
    
    bool Propagate() {
        while (stacksize > 0) {
            (int i1, int t1) = stack[stacksize - 1];
            stacksize--;

            // Get the x and y coordinate of the pixel in the output image.
            int x1 = i1 % MX;
            int y1 = i1 / MX;

            // For each direction (up/left/down/right)...
            for (int d = 0; d < 4; d++) {
                // Get the coordinate of the adjacent pixel.
                int x2 = x1 + dx[d];
                int y2 = y1 + dy[d];

                // If the output doesn't need to be periodic, ignore this 
                // direction (e.g. don't look at the pixel to left of the 
                // leftmost pixel, cuz there ain't one!)
                if (!periodic && (x2 < 0 || y2 < 0 || x2 + N > MX || y2 + N > MY)) continue;

                // Otherwise, if this pixel is off the edge, go to the other side.
                if (x2 < 0) x2 += MX;
                else if (x2 >= MX) x2 -= MX;
                if (y2 < 0) y2 += MY;
                else if (y2 >= MY) y2 -= MY;

                // Convert the pixel coordinates to the 1-dimensional index.
                int i2 = x2 + y2 * MX;

                // Eliminate the choices that aren't valid anymore, and for every
                // neighbour that gets impacted, add them to the stack too so it
                // "propagates" (they get checked as well).
                int[] p = propagator[d][t1];
                int[][] compat = compatible[i2];
                for (int l = 0; l < p.Length; l++) {
                    int t2 = p[l];
                    int[] comp = compat[t2];

                    comp[d]--;
                    if (comp[d] == 0) Ban(i2, t2);
                }
            }
        }

        // Will be false if there's a contradiction (i.e. because the choices
        // we've made, there's no way to create a valid image anymore).
        return sumsOfOnes[0] > 0;
    }

    public long[] Save() {
        long[] bitmap = new long[MX * MY];

        // If it hasn't finished making the full image??...
        if (observed[0] >= 0) {
            for (int y = 0; y < MY; y++) {
                int dy = y < MY - N + 1 ? 0 : N - 1;
                for (int x = 0; x < MX; x++) {
                    int dx = x < MX - N + 1 ? 0 : N - 1;
                    
                    // Find the pattern from the observed array for this pixel.
                    byte[] pattern = patterns[observed[x - dx + (y - dy) * MX]];

                    // Get color of the top-left pixel?? of the chosen pattern, 
                    // based on its color index.
                    bitmap[x + y * MX] = colors[pattern[dx + dy * N]];
                }
            }
        }
        else {
            // The blurry animation.
            for (int i = 0; i < wave.Length; i++) {
                int contributors = 0, r = 0, g = 0, b = 0;
                int x = i % MX, y = i / MX;

                int s = x + y * MX;
                if (!periodic && (x + 1 > MX || y + 1 > MY || x < 0 || y < 0)) continue;
                for (int t = 0; t < T; t++) {
                    if (wave[s][t]) {
                        contributors++;
                        long argb = colors[patterns[t][0]];
                        r += (int) ((argb & 0xff0000) >> 16);
                        g += (int) ((argb & 0xff00) >> 8);
                        b += (int) (argb & 0xff);
                    }
                }
                bitmap[i] = unchecked((int)0xff000000 | ((r / contributors) << 16) | ((g / contributors) << 8) | b / contributors);
            }
        }
        return bitmap;
    }

    static int RandomFromWeights(double[] weights, double r) {
        double sum = 0;
        for (int i = 0; i < weights.Length; i++) {
          sum += weights[i];
        }
        double threshold = r * sum;

        double partialSum = 0;
        for (int i = 0; i < weights.Length; i++) {
            partialSum += weights[i];
            if (partialSum >= threshold) return i;
        }
        return 0;
    }
}
