using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallTilePlacer : MonoBehaviour {
    public Tilemap wallsTilemap;
    public Tilemap stoneGroundTilemap;

    public Tile wallTileBase;
    public Tile wallTileBaseLeft;
    public Tile wallTileBaseRight;
    public Tile wallTileBaseShadow;
    public Tile wallTileMid;
    public Tile wallTileMidLeft;
    public Tile wallTileMidRight;
    public Tile wallTileMidShadow;
    public Tile wallTileRidgeTop;
    public Tile wallTileRidgeTopLeft;
    public Tile wallTileRidgeTopRight;
    public Tile wallTileRidgeLeft;
    public Tile wallTileRidgeRight;
    public Tile wallTileRidgeJoinLeft;
    public Tile wallTileRidgeJoinRight;
    public Tile wallTileRidgeInnerLeftCorner;
    public Tile wallTileRidgeInnerRightCorner;
    public Tile stoneGroundTile;
    public Tile stoneGroundTileLeftCorner;
    public Tile stoneGroundTileRightCorner;

    public void Run(LevelGenerator generator) {

        for (int y = generator.Y1; y < generator.Y2; y++) {
            for (int x = generator.X1; x < generator.X2; x++) {
                PlaceTilesFor(generator, x, y, generator.bossRoomPos);
            }
        }
    }

    void PlaceTilesFor(LevelGenerator generator, int x, int y, List<Vector3Int> bossRoomPos) {
        bool atMe = generator.HasWallAt(x, y);
        if (!atMe) { return; }

        bool belowMe = generator.HasWallAt(x, y - 1);
        bool twoBelowMe = generator.HasWallAt(x, y - 2);
        bool aboveMe = generator.HasWallAt(x, y + 1);
        bool myLeft = generator.HasWallAt(x - 1, y);
        bool myRight = generator.HasWallAt(x + 1, y);
        bool myUpperLeft = generator.HasWallAt(x - 1, y + 1);
        bool myUpperRight = generator.HasWallAt(x + 1, y + 1);

        Vector3Int temp = new Vector3Int(x, y);
        //Debug.Log(" X " + x + " Y " + y);
        // Place a stone ground tile 2 squares above me. Use a corner piece if
        // appropriate.
        if (!myLeft && !aboveMe) {
            stoneGroundTilemap.SetTile(new Vector3Int(x, y + 2), stoneGroundTileLeftCorner);
        }
        else if (!myRight && !aboveMe) {
            stoneGroundTilemap.SetTile(new Vector3Int(x, y + 2), stoneGroundTileRightCorner);
        }
        else {
            //bossroomarray - conatins a list of the co ordinate points for the boss room created in the Create_World() method
            //if(x, y in bossrommarray){ Dont place the stone ground} else{ the statement below}
            
            if(bossRoomPos.Contains(temp) == false)
                stoneGroundTilemap.SetTile(new Vector3Int(x, y + 2), stoneGroundTile); //Stone ground or outer frame tiles
        }

        if (!belowMe) {
            // The mid tile to use.
            if (!myLeft) {
                wallsTilemap.SetTile(new Vector3Int(x, y + 1), wallTileMidLeft);
            }
            else if (!myRight) {
                wallsTilemap.SetTile(new Vector3Int(x, y + 1), wallTileMidRight);
            }
            else {
                bool myLowerLeft = generator.HasWallAt(x - 1, y - 1);
                wallsTilemap.SetTile(new Vector3Int(x, y + 1), myLowerLeft ? wallTileMidShadow : wallTileMid);
            }
        }
        if (!belowMe && !twoBelowMe) {
            // The base tile to use.
            if (!myLeft) {
                wallsTilemap.SetTile(new Vector3Int(x, y), wallTileBaseLeft);
            }
            else if (!myRight) {
                wallsTilemap.SetTile(new Vector3Int(x, y), wallTileBaseRight);
            }
            else {
                bool myLowerLeft = generator.HasWallAt(x - 1, y - 1);
                wallsTilemap.SetTile(new Vector3Int(x, y), myLowerLeft ? wallTileBaseShadow : wallTileBase);
            }
        }
        if (!aboveMe) {
            // If there's nothing, place a ridge 2 squares above me.
            if (!myLeft) {
                wallsTilemap.SetTile(new Vector3Int(x, y + 2), wallTileRidgeTopLeft);
            }
            else if (!myRight) {
                wallsTilemap.SetTile(new Vector3Int(x, y + 2), wallTileRidgeTopRight);
            }
            else {
                wallsTilemap.SetTile(new Vector3Int(x, y + 2), wallTileRidgeTop);
            }
        }
        else {
            // Otherwise, consider if there's space to my left or right, and
            // place a ridge tile appropriately.
            if (!myLeft) {
                if (bossRoomPos.Contains(temp) == false)
                    wallsTilemap.SetTile(new Vector3Int(x, y + 2), myUpperLeft ? wallTileRidgeJoinLeft : wallTileRidgeLeft);
            }
            else if (!myRight) {
                if(bossRoomPos.Contains(temp) == false)
                    wallsTilemap.SetTile(new Vector3Int(x, y + 2), myUpperRight ? wallTileRidgeJoinRight : wallTileRidgeRight);
            }
            else if (!myUpperLeft) {
                if (bossRoomPos.Contains(temp) == false)
                    wallsTilemap.SetTile(new Vector3Int(x, y + 2), wallTileRidgeInnerLeftCorner);
            }
            else if (!myUpperRight) {
                if (bossRoomPos.Contains(temp) == false)
                    wallsTilemap.SetTile(new Vector3Int(x, y + 2), wallTileRidgeInnerRightCorner);
            }
        }
    }
}
