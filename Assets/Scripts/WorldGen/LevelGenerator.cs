using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {
    const long openSampleColor = 0xFFFFFFFF;
    const long wallSampleColor = 0xFF000000;
    const long puddleSampleColor = 0xFF0070FF;

    public GameObject astargrid;
    public GroundTilePlacer groundTilePlacer;
    public WallTilePlacer wallTilePlacer;
    public PuddleTilePlacer puddleTilePlacer;
    public RemoveEnclosed removeEnclosed;
    public PlayerSpawner Player;
    public BossSpawner Boss;
    public BossPlayerDetector Detector;
    public WorldGenUI worldGenUI;
    public InGameUI gameUI;

    int seed;

    public int mazeWidth;
    public int mazeHeight;
    public int outsideWallThickness;
    public int outsideExtraGrass;

    public int wfcWallN;
    public int wfcWallSymmetry;
    public Texture2D wfcWallSample;

    public int wfcPuddleN;
    public int wfcPuddleSymmetry;
    public Texture2D wfcPuddleSample;

    public int minWallArea;
    public int minPuddleArea;
    
    long[] wfcWallOutput;
    long[] wfcPuddleOutput;

    public int X1 => -outsideWallThickness - outsideExtraGrass;
    public int Y1 => -outsideWallThickness - outsideExtraGrass;
    public int X2 => mazeWidth + outsideWallThickness + outsideExtraGrass;
    public int Y2 => mazeHeight + outsideWallThickness + outsideExtraGrass;

    public int bossSide;
    public int X, Y;
    public List<Vector3Int> bossRoomPos;

    void Start() {
        this.bossSide = this.genBossSide();
        this.bossRoomPos = this.bossRoomArray();
        StartCoroutine(CreateWorld());
    }

    IEnumerator CreateWorld() {
        seed = MainMenuController.seed;

        // know seed when error
        print(seed);

        yield return new WaitForSeconds(0);

        // Generate Map
        yield return RunWFC();
        DoWFCPostProcessing();
        removeEnclosed.FreeBossRoom();

        // Scripts that determine which exact tile from the tilemaps to use.
        groundTilePlacer.Run(this);
        wallTilePlacer.Run(this);
        puddleTilePlacer.Run(this);

        // Generate A* Grid
        yield return new WaitForSeconds(1);
        astargrid.GetComponent<AStarGrid>().CreateGrid();

        // Generate Player, Decorations and Minion Spawners
        yield return new WaitForSeconds(.5f);
        MinionSpawning.Generate();
        StructureGen.Generate();
        CampFireGen.Generate();
        Boss.Generate(X, Y, bossSide);
        Player.Generate();
        Detector.setPosition();
        worldGenUI.Hide();

        yield return new WaitForSeconds(6);
        gameUI.playerHealthBarScript.Show();
    }

    void DoWFCPostProcessing() {
        wfcWallOutput = removeEnclosed.Run(this, wfcWallOutput);

        worldGenUI.SubmitImage(wfcWallOutput, wfcPuddleOutput);

        if (minWallArea > 0) {
            AreaBasedCleanup.Run(wfcWallOutput, mazeWidth, mazeHeight, wallSampleColor, openSampleColor, minWallArea, true);
        }
        
        worldGenUI.SubmitImage(wfcWallOutput, wfcPuddleOutput);

        if (minPuddleArea > 0) {
            AreaBasedCleanup.Run(wfcPuddleOutput, mazeWidth, mazeHeight, puddleSampleColor, openSampleColor, minPuddleArea, false);
        }

        worldGenUI.SubmitImage(wfcWallOutput, wfcPuddleOutput);
    }

    IEnumerator RunWFC() {
        WaveFunctionCollapse wallWfc = new WaveFunctionCollapse(
            wfcWallSample, mazeWidth, mazeHeight, wfcWallN, false, true,
            wfcWallSymmetry, WaveFunctionCollapse.Heuristic.Entropy
        );
        WaveFunctionCollapse puddleWfc = new WaveFunctionCollapse(
            wfcPuddleSample, mazeWidth, mazeHeight, wfcPuddleN, false, true,
            wfcPuddleSymmetry, WaveFunctionCollapse.Heuristic.Entropy
        );

        // Generate the blank starting images.
        bool wallsDone = wallWfc.Start(seed, 0);
        bool puddlesDone = puddleWfc.Start(seed, 0);
        wfcWallOutput = wallWfc.Save();
        wfcPuddleOutput = puddleWfc.Save();

        worldGenUI.Prep(mazeWidth, mazeHeight);
        worldGenUI.SubmitImage(wfcWallOutput, wfcPuddleOutput);

        while (!wallsDone) {
            wallsDone = wallWfc.Continue(100);
            wfcWallOutput = wallWfc.Save();
            worldGenUI.SubmitImage(wfcWallOutput, wfcPuddleOutput);

            // Break execution so Unity has a chance to breathe and re-render 
            // the animation image ;)
            yield return new WaitForSeconds(0);
        }
        while (!puddlesDone) {
            puddlesDone = puddleWfc.Continue(100);
            wfcPuddleOutput = puddleWfc.Save();
            worldGenUI.SubmitImage(wfcWallOutput, wfcPuddleOutput);

            // Break execution so Unity has a chance to breathe and re-render 
            // the animation image ;)
            yield return new WaitForSeconds(0);
        }

        worldGenUI.SubmitImage(wfcWallOutput, wfcPuddleOutput);
    }

    public bool HasWallAt(int x, int y) {
        // If this position is beyond the outer wall, do not treat it as a wall.
        if (x < -outsideWallThickness ||
            x >= mazeWidth + outsideWallThickness ||
            y < -outsideWallThickness ||
            y >= mazeHeight + outsideWallThickness) {

            return false;
        }

        // If this position is beyond the maze area, make it part of the outer wall unless it is part of the Boss room.
        if (x < 0 || x >= mazeWidth || y < 0 || y >= mazeHeight) {
            return bossRoomPos.Contains(new Vector3Int(x, y)) ? false : true;
        }

        // Otherwise check if its not part of the Boss room before listening to the WFC output.
        return bossRoomPos.Contains(new Vector3Int(x, y)) ? false : wfcWallOutput[y * mazeWidth + x] == wallSampleColor;
    }

    public bool HasPuddleAt(int x, int y)
    {
        // Don't attempt to place puddles in the outer wall.
        if (x < 0 || x >= mazeWidth || y < 0 || y >= mazeHeight)
        {
            return false;
        }

        // Don't attempt to place puddles under any wall tiles.
        if (HasWallAt(x, y) || HasWallAt(x, y - 1) || HasWallAt(x, y - 2))
        {
            return false;
        }

        // Otherwise listen to the WFC output.
        return wfcPuddleOutput[y * mazeWidth + x] == puddleSampleColor;
    }

    int genBossSide()
    {
        return Random.Range(0, 4);
    }

    List<Vector3Int> calc_bossRoomArray(int x, int y)
    {
        /*
         * This method is a refractored method for the 'bossRoomArray(int, int)' to add each Tile co ordinates to the array
         * */
        int incrementX = 1, incrementY = 1;
        int boardSizeX = 0, boardSizeY = 0;

        if (this.bossSide == 0)
            incrementX = -1;
        if(this.bossSide == 2)
            incrementY = -1;

        if (this.bossSide <= 1)
        {
            boardSizeX = 17;
            boardSizeY = 20;
        }
        else
        {
            boardSizeX = 20;
            boardSizeY = 17;
        }

        List<Vector3Int> bRoom = new List<Vector3Int>();
        int i = 1;
        while (i < boardSizeX)
        {
            int j = 1;
            while (j <= boardSizeY)
            {
                bRoom.Add(new Vector3Int(x, y));
                y += incrementY;
                j++;
            }
            x += incrementX;
            i++;
        }
        return bRoom;
    }


    List<Vector3Int> bossRoomArray()
    {
        /*
         * The method is used to create a boss room based on the Level's side.
         */
        if (this.bossSide == 0) //Left Side
        {
            //Debug.Log("Left Side");
            X = -3;
            Y = Random.Range(3, 71);
            
        }
        else if(this.bossSide == 1)
        {
            //Debug.Log("Right Side");
            X = 98;
            Y = Random.Range(3, 71);
            
        }
        else if (this.bossSide == 2)
        {
            //Debug.Log("Bottom");
            X = Random.Range(10, 50);
            Y = -2;
            
        }
        else
        {
            //Debug.Log("Top");
            X = Random.Range(10, 50);
            Y = 98;
        }

        return calc_bossRoomArray(X, Y);
    }
}

