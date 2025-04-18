using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GroundTilePlacer : MonoBehaviour {
    public Tilemap groundTilemap;

    public Tile[] groundTiles;

    public void Run(LevelGenerator generator) {
        System.Random groundTileRandom = new System.Random(MainMenuController.seed);

        // Each square gets a random grass tile (based on the seed).
        for (int y = generator.Y1; y < generator.Y2; y++) {
            for (int x = generator.X1; x < generator.X2; x++) {
                Tile groundTile = groundTiles[groundTileRandom.Next(groundTiles.Length)];
                groundTilemap.SetTile(new Vector3Int(x, y), groundTile);
            }
        }
    }
}
