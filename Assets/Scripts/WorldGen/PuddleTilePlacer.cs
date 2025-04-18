using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuddleTilePlacer : MonoBehaviour {
    public Tilemap puddlesTilemap;
    
    public Tile puddleTileCenter;
    public Tile puddleTileEdge;
    public Tile puddleTileCorner;
    public Tile puddleTileThinCorner;
    public Tile puddleTileThin;
    public Tile puddleTileThinCap;
    public Tile puddleTileSingle;
    public Tile puddleTileTIntersection;
    public Tile puddleTilePlusIntersection;
    public Tile puddleTileInnerCorner;
    public Tile puddleTileEdgedInnerCornerLeft;
    public Tile puddleTileEdgedInnerCornerRight;
    public Tile puddleTileTAdjacent;
    public Tile puddleTileChunkyPlusIntersection;
    public Tile puddleTileDoubleChunkyPlusIntersection;

    Dictionary<string, Tile> choices;

    public void Run(LevelGenerator generator) {
        // The adjacency rules for each type of tile. Only one of each type of
        // tile (ignoring rotation) is required.
        choices = new() {
            // ↑↗→↘↓↙←↖ -- # means yes, ? means maybe, space means no.
            { "########", puddleTileCenter },
            { " ?#####?", puddleTileEdge },
            { " ?###? ?", puddleTileCorner },
            { " ?# #? ?", puddleTileThinCorner },
            { "#? ?#? ?", puddleTileThin },
            { " ? ?#? ?", puddleTileThinCap },
            { " ? ? ? ?", puddleTileSingle },
            { " ?# # #?", puddleTileTIntersection },
            { "# # # # ", puddleTilePlusIntersection },
            { "### ####", puddleTileInnerCorner },
            { " ?### #?", puddleTileEdgedInnerCornerLeft },
            { " ?# ###?", puddleTileEdgedInnerCornerRight },
            { "### # ##", puddleTileTAdjacent },
            { "# # # ##", puddleTileChunkyPlusIntersection },
            { "# ### ##", puddleTileDoubleChunkyPlusIntersection },
        };

        for (int y = generator.Y1; y < generator.Y2; y++) {
            for (int x = generator.X1; x < generator.X2; x++) {
                PlaceTileFor(generator, x, y);
            }
        }
    }

    void PlaceTileFor(LevelGenerator generator, int x, int y) {
        bool atMe = generator.HasPuddleAt(x, y);
        if (!atMe) { return; }

        // Work out which neighbouring cells only have puddles.
        bool n = generator.HasPuddleAt(x, y + 1);
        bool ne = generator.HasPuddleAt(x + 1, y + 1);
        bool e = generator.HasPuddleAt(x + 1, y);
        bool se = generator.HasPuddleAt(x + 1, y - 1);
        bool s = generator.HasPuddleAt(x, y - 1);
        bool sw = generator.HasPuddleAt(x - 1, y - 1);
        bool w = generator.HasPuddleAt(x - 1, y);
        bool nw = generator.HasPuddleAt(x - 1, y + 1);

        // For each rotation direction...
        for (int d = 0; d < 4; d++) {
            foreach (var choice in choices) {
                // Get the character for this direction. Shift forward by 2
                // letters for each direction (90 degree rotation).
                char choiceN = choice.Key[(2 * d + 0) % 8];
                char choiceNE = choice.Key[(2 * d + 1) % 8];
                char choiceE = choice.Key[(2 * d + 2) % 8];
                char choiceSE = choice.Key[(2 * d + 3) % 8];
                char choiceS = choice.Key[(2 * d + 4) % 8];
                char choiceSW = choice.Key[(2 * d + 5) % 8];
                char choiceW = choice.Key[(2 * d + 6) % 8];
                char choiceNW = choice.Key[(2 * d + 7) % 8];

                // Check if this tile is compatible with the neighbours in this
                // location.
                if (Matches(n, choiceN) && 
                    Matches(ne, choiceNE) && 
                    Matches(e, choiceE) && 
                    Matches(se, choiceSE) && 
                    Matches(s, choiceS) && 
                    Matches(sw, choiceSW) && 
                    Matches(w, choiceW) && 
                    Matches(nw, choiceNW)) {
                
                    SetTile(x, y, choice.Value, d * 90);
                    return;
                }
            }
        }

        // Fallback to using the single tile if the above fails. (Hopefully 
        // never!)
        SetTile(x, y, puddleTileSingle, 0);
    }

    bool Matches(bool reality, char choice) {
        // ? characters match either, otherwise "#" for true and " " for false.
        return choice == '?' || choice == (reality ? '#' : ' ');
    }

    void SetTile(int x, int y, Tile tile, int rotation) {
        TileChangeData data = new TileChangeData(
            new Vector3Int(x, y), 
            tile,
            Color.white,
            Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotation))
        );
        puddlesTilemap.SetTile(data, false);
    }
}
