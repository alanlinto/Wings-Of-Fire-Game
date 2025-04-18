using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoveEnclosed : MonoBehaviour
{
  const long openSampleColor = 0xFFFFFFFF;
  const long wallSampleColor = 0xFF000000;

  public List<Vector2Int> queue = new List<Vector2Int>();

  public HashSet<Vector2Int> Explored = new HashSet<Vector2Int>();

  public List<Vector2Int> unexplored = new List<Vector2Int>();
  public List<Vector2Int> explored = new List<Vector2Int>();

  private List<Vector2Int> onePixelList = new List<Vector2Int>();

  public List<Vector2Int> walls = new List<Vector2Int>();


  private LevelGenerator lg;

  private int mazeHeight;
  private int mazeWidth;

  private int wallIndex = 0;

  long[] wfcWallOutput;

  Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };


  public long[] Run(LevelGenerator generator, long[] array)
  {
    lg = generator;
    this.mazeWidth = generator.mazeWidth;
    this.mazeHeight = generator.mazeHeight;

    wfcWallOutput = array;

    findEnclosed();
    while (unexplored.Count > 0)
    {
      openWall();
    }

    return wfcWallOutput;
  }

  private void findEnclosed()
  {
    // Adds all values to unexplored section that are not walls
    for (int i = 0; i != mazeHeight; ++i)
    {
      for (int j = 0; j != mazeWidth; ++j)
      {
        if (!lg.HasWallAt(j, i) && !unexplored.Contains(new Vector2Int(j, i)))
        {
          //removeOnePixel(j, i);

          unexplored.Add(new Vector2Int(j, i));
        }
      }
    }

    // starts at (0,0) in WFC output array and will increase x and y
    // until it finds a spot that is not a wall
    int x = 0;
    int y = 0;
    while (lg.HasWallAt(x, y))
    {
      ++x;
      ++y;
    }

    FloodFill(x, y);
  }

  private void FloodFill(int x, int y)
  {
    Vector2Int start = new Vector2Int(x, y);

    queue.Add(start);

    // if current coord unexplored, add to explored list
    // and add surrounding tiles to queue to explore
    while (queue.Count > 0)
    {
      Vector2Int current = queue[0];
      if (!explored.Contains(current))
      {
        // remove it if it is only one pixel of free space
        removeOnePixel(current.x, current.y);

        explored.Add(current);
        Vector2Int[] neighbours = findNeighbours(current.x, current.y);

        foreach (Vector2Int n in neighbours)
        {
          queue.Add(n);
        }
      }
      queue.RemoveAt(0);
    }

    // remove explored tiles from unexplored list
    unexplored = unexplored.Except(explored).ToList();
  }

  private Vector2Int[] findNeighbours(int x, int y)
  {
    List<Vector2Int> n = new List<Vector2Int>();

    int up = y + 1;
    int down = y - 1;
    int left = x - 1;
    int right = x + 1;

    // if neighbouring position is in bounds and not a wall
    // add it as a viable neighbour
    if (up >= 0 && !lg.HasWallAt(x, up))
    {
      n.Add(new Vector2Int(x, up));
    }

    if (down < mazeHeight && !lg.HasWallAt(x, down))
    {
      n.Add(new Vector2Int(x, down));
    }

    if (left >= 0 && !lg.HasWallAt(left, y))
    {
      n.Add(new Vector2Int(left, y));
    }

    if (right < mazeWidth && !lg.HasWallAt(right, y))
    {
      n.Add(new Vector2Int(right, y));
    }

    return n.ToArray();
  }

  private void removeOnePixel(int x, int y)
  {
    Vector2Int belowMe = new Vector2Int(x, y - 1);
    Vector2Int aboveMe = new Vector2Int(x, y + 1);
    // if vertical wall has two spaces it may be inaccessible, so it is also treated as one pixel
    Vector2Int twoAboveMe = new Vector2Int(x, y + 2);
    Vector2Int myLeft = new Vector2Int(x - 1, y);
    Vector2Int myRight = new Vector2Int(x + 1, y);

    int index = y * mazeWidth + x;

    // if any open space pixel has a wall pixel directly above and below and/or left and right of it
    bool onePixel = (lg.HasWallAt(belowMe.x, belowMe.y) && (lg.HasWallAt(aboveMe.x, aboveMe.y) || lg.HasWallAt(twoAboveMe.x, twoAboveMe.y))
        || (lg.HasWallAt(myLeft.x, myLeft.y) && lg.HasWallAt(myRight.x, myRight.y)));


    if (onePixel && wfcWallOutput[index] != wallSampleColor)
    {
      //remove surrounding walls
      getWall(belowMe, Vector2Int.down);
      getWall(aboveMe, Vector2Int.up);
      getWall(myLeft, Vector2Int.left);
      getWall(myRight, Vector2Int.right);
    }
  }

  private void getWall(Vector2Int vector, Vector2Int vector2)
  {
    Vector2Int wall = vector + vector2;
    //Vector2Int wall2 = vector + (2 * vector2);

    removeWall(vector);
    removeWall(wall);
    //removeWall(wall2);
  }



  private void openWall()
  {
    if (unexplored.Count != 0)
    {
      Vector2Int randomPoint;
      List<Vector2Int> list;

      // Chooses a random point in the smaller list
      // otherwise larger one causes more chance for error
      if (unexplored.Count <= explored.Count)
      {
        int index = Random.Range(0, unexplored.Count);

        randomPoint = unexplored[index];
        list = explored;
      }
      else
      {
        int index = Random.Range(0, explored.Count);

        randomPoint = explored[index];
        list = unexplored;
      }

      // get the nearest point that is in the other list in +-x and +-y directions
      // e.g. if random point is chosen in unexplored list, looks for closest points in explored list
      walls.Add(goToNearest(randomPoint, Vector2Int.up, list));
      walls.Add(goToNearest(randomPoint, Vector2Int.right, list));
      walls.Add(goToNearest(randomPoint, Vector2Int.down, list));
      walls.Add(goToNearest(randomPoint, Vector2Int.left, list));

      Vector2Int wallPoint = Vector2Int.zero;
      float minDistance = Mathf.Infinity;

      // finds the point in other list that is the closest, which is determined as the "wallPoint".
      for (int i = 0; i != walls.Count; ++i)
      {
        if (walls[i].x >= 0 && walls[i].x < mazeWidth && walls[i].y >= 0 && walls[i].y < mazeHeight)
        {

          float distance = (walls[i] - randomPoint).sqrMagnitude;

          if (distance < minDistance)
          {
            minDistance = distance;
            wallPoint = walls[i];
            // gets direction that the wall point is in
            wallIndex = i;
          }
        }

      }

      // gets the point on the other side of the wall in the opposite direction
      Vector2Int otherSide = wallPoint - directions[wallIndex];

      while (lg.HasWallAt(otherSide.x, otherSide.y))
      {
        otherSide -= directions[wallIndex];
      }

      // if direction that the wall point from the random point is +-y,
      // remove main wall and surrounding walls +-x of the wall point
      if (wallIndex == 0 || wallIndex == 2)
      {

        removeWalls(wallPoint, otherSide, Vector2Int.left, Vector2Int.right);

      }
      // else remove main wall and surrounding walls +-y of the wall point 
      else if (wallIndex == 1 || wallIndex == 3)
      {
        removeWalls(wallPoint, otherSide, Vector2Int.up, Vector2Int.down);

      }

      // floodfill from the unexplored list
      if (unexplored.Count <= explored.Count)
      {
        FloodFill(randomPoint.x, randomPoint.y);
      }
      else
      {
        FloodFill(wallPoint.x, wallPoint.y);
      }

      walls.Clear();
    }

  }

  private void removeWalls(Vector2Int wallPoint, Vector2Int otherSide, Vector2Int direction1, Vector2Int direction2)
  {
    // gets distance to perpendicular walls from the wallpoint 
    float left1 = Vector2.Distance(wallPoint, goToWall(wallPoint, direction1));
    float right1 = Vector2.Distance(wallPoint, goToWall(wallPoint, direction2));

    // gets distance to perpendicular walls from the otherside 
    float left2 = Vector2.Distance(otherSide, goToWall(otherSide, direction1));
    float right2 = Vector2.Distance(otherSide, goToWall(otherSide, direction2));

    // looks the the same direction for the wallpoint and otherside and chooses the smaller one
    float smallerLeft = left1 < left2 ? left1 : left2;
    float smallerRight = right1 < right2 ? right1 : right2;

    // chooses the largest distance and direction to destroy walls
    int maxAmount = smallerLeft > smallerRight ? (int)smallerLeft : (int)smallerRight;

    //int toCut = Random.Range(2, maxAmount);

    for (int i = 0; i != maxAmount; ++i)
    {

      Vector2Int toRemove = wallPoint - directions[wallIndex];

      while (lg.HasWallAt(toRemove.x, toRemove.y))
      {
        removeWall(toRemove);
        toRemove -= directions[wallIndex];
      }

      if (smallerLeft > smallerRight)
      {
        wallPoint += direction1;
      }
      else
      {
        wallPoint += direction2;
      }
    }
  }

  private void removeWall(Vector2Int p)
  {
    // if in bounds and it is a wall in the WFC array, change it to the open colour
    if ((p.x >= 0 && p.x < mazeWidth) && (p.y >= 0 && p.y < mazeHeight))
    {
      if (wfcWallOutput[p.y * mazeWidth + p.x] == wallSampleColor)
      {
        wfcWallOutput[p.y * mazeWidth + p.x] = openSampleColor;
      }
    }
  }

  private Vector2Int goToNearest(Vector2Int point, Vector2Int direction, List<Vector2Int> list)
  {
    // keep going in a direction until the x and y are contained in the list
    if ((point.x >= 0 && point.x < mazeWidth) && (point.y >= 0 && point.y < mazeHeight))
    {
      if (!list.Contains(point))
      {
        point = goToNearest(point + direction, direction, list);

      }
    }

    return point;
  }

  private Vector2Int goToWall(Vector2Int point, Vector2Int direction)
  {
    // keep going in direction until there is a wall
    //if ((point.x >= 0 && point.x < mazeWidth) && (point.y >= 0 && point.y < mazeHeight))
    //{
    if (!lg.HasWallAt(point.x, point.y))
    {
      point = goToWall(point + direction, direction);

    }
    //}

    return point;
  }

  public void FreeBossRoom()
  {
    bool AbleToCut = true;

    // Get a random room point
    int index = Random.Range(0, lg.bossRoomPos.Count);
    Vector3Int randomPoint = lg.bossRoomPos[index];

    print(randomPoint);

    // Find closest point prioriting points with a shared axis
    Vector3Int shortestPoint = Vector3Int.zero;
    float minDistance = Mathf.Infinity;

    foreach (Vector3Int vector in explored)
    {
      float distance = (vector - randomPoint).sqrMagnitude;

      if (distance < minDistance && vector != randomPoint)
      {
        minDistance = distance;
        shortestPoint = vector;
      }
      else if (distance == minDistance)
      {
        if (vector.x == randomPoint.x || vector.y == randomPoint.y)
        {
          shortestPoint = vector;
        }
      }
    }

    // Decided the Axis and Direction to cut the wall
    bool CutWallHorizontally = (randomPoint - new Vector3Int(randomPoint.x, shortestPoint.y)).sqrMagnitude < (randomPoint - new Vector3Int(shortestPoint.x, randomPoint.y)).sqrMagnitude;
    bool CutRight = randomPoint.x <= shortestPoint.x;
    bool CutUp = randomPoint.y <= shortestPoint.y;
    Vector3Int direction = CutWallHorizontally
      ? (CutRight ? Vector3Int.right : Vector3Int.left)
      : (CutUp ? Vector3Int.up : Vector3Int.down);

    // Move point to closest wall
    while (!lg.HasWallAt(randomPoint.x, randomPoint.y))
    {
      randomPoint += direction;
    }


    if (CutWallHorizontally)
    {
      // Cutting 4 rows
      int WallThickness = 0;
      int AboveThickness = 0;
      int BelowThickness = 0;
      int BelowBelowThickness = 0;

      // Check if a path can be made if the walls are cut
      if (lg.HasWallAt((randomPoint - direction).x, (randomPoint + Vector3Int.up).y)
        || lg.HasWallAt((randomPoint - direction).x, (randomPoint + Vector3Int.down).y)
        || lg.HasWallAt((randomPoint - direction).x, (randomPoint + Vector3Int.down * 2).y))
      {
        AbleToCut = false;
      }

      // Check if the walls being cut share the same thickness
      while (lg.HasWallAt(randomPoint.x, randomPoint.y) && AbleToCut)
      {
        WallThickness++;
        if (lg.HasWallAt(randomPoint.x, (randomPoint + Vector3Int.up).y))
        {
          AboveThickness++;
        }
        if (lg.HasWallAt(randomPoint.x, (randomPoint + Vector3Int.down).y))
        {
          BelowThickness++;
        }
        if (lg.HasWallAt(randomPoint.x, (randomPoint + Vector3Int.down * 2).y))
        {
          BelowBelowThickness++;
        }
        randomPoint += direction;
      }

      // Check that if doesn't weirdly affect the appearance if cut
      if (WallThickness != AboveThickness
        || WallThickness != BelowThickness
        || WallThickness != BelowBelowThickness)
      {
        AbleToCut = false;
      }
      else if (lg.HasWallAt(randomPoint.x, (randomPoint + Vector3Int.up).y)
              || (lg.HasWallAt(randomPoint.x, (randomPoint + Vector3Int.down).y))
              || (lg.HasWallAt(randomPoint.x, (randomPoint + Vector3Int.down * 2).y)))
      {
        AbleToCut = false;
      }

      if (AbleToCut)
      {
        Vector3Int CuttingPoint = randomPoint - WallThickness * direction;
        while (lg.HasWallAt(CuttingPoint.x, CuttingPoint.y))
        {
          lg.bossRoomPos.Add(CuttingPoint);
          lg.bossRoomPos.Add(new Vector3Int(CuttingPoint.x, (CuttingPoint + Vector3Int.up).y));
          lg.bossRoomPos.Add(new Vector3Int(CuttingPoint.x, (CuttingPoint + Vector3Int.down).y));
          lg.bossRoomPos.Add(new Vector3Int(CuttingPoint.x, (CuttingPoint + Vector3Int.down * 2).y));

          CuttingPoint += direction;
        }
      }

    }
    else
    {
      // Cutting 2 columns
      int WallThickness = 0;
      int NextThickness = 0;
      Vector3Int WallNext = Vector3Int.zero;

      // Check if a path can be made if the walls are cut
      if (lg.HasWallAt((randomPoint + Vector3Int.left).x, (randomPoint - direction).y)
        && lg.HasWallAt((randomPoint + Vector3Int.right).x, (randomPoint - direction).y))
      {
        AbleToCut = false;
      }

      // Select the second column the wall should cut
      WallNext = lg.HasWallAt((randomPoint + Vector3Int.left).x, (randomPoint - direction).y) ? Vector3Int.right : Vector3Int.left;

      // Check if the walls being cut share the same thickness
      while (lg.HasWallAt(randomPoint.x, randomPoint.y) && AbleToCut)
      {
        WallThickness++;
        if (lg.HasWallAt((randomPoint + WallNext).x, randomPoint.y))
        {
          NextThickness++;
        }
        randomPoint += direction;
      }

      // Check that if doesn't weirdly affect the appearance if cut
      if (WallThickness != NextThickness)
      {
        AbleToCut = false;
      }
      else if (lg.HasWallAt((randomPoint + WallNext).x, randomPoint.y))
      {
        AbleToCut = false;
      }

      if (AbleToCut)
      {
        Vector3Int CuttingPoint = randomPoint - WallThickness * direction;
        while (lg.HasWallAt(CuttingPoint.x, CuttingPoint.y))
        {
          lg.bossRoomPos.Add(CuttingPoint);
          lg.bossRoomPos.Add(new Vector3Int((CuttingPoint + WallNext).x, CuttingPoint.y));

          CuttingPoint += direction;
        }
      }
    }

    if (!AbleToCut)
    {
      FreeBossRoom();
    }
  }

  //void OnDrawGizmos()
  //{
  //    Gizmos.DrawWireCube(new Vector2(50, 50), new Vector2(100, 100));

  //    Gizmos.color = Color.red;

  //    foreach (Vector2 vector in onePixelList)
  //    {
  //        Gizmos.DrawCube(new Vector3(vector.x + 0.5f, vector.y + 2.5f, 0), Vector3.one);
  //    }


  //    Gizmos.color = Color.yellow;

  //    foreach (Vector2 vector in unexplored)
  //    {
  //        Gizmos.DrawCube(new Vector3(vector.x + 0.5f, vector.y + 2.5f, 0), Vector3.one);
  //    }

  //    Gizmos.color = Color.green;

  //    foreach (Vector2 vector in explored)
  //    {
  //        Gizmos.DrawCube(new Vector3(vector.x + 0.5f, vector.y + 2.5f, 0), Vector3.one);
  //    }
  //}
}
