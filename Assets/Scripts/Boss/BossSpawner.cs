using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public void Generate(int x, int y, int side)
    {
        gameObject.transform.position = bossPos(x, y, side);
    }

    Vector3 bossPos(int  x, int y, int side)
    {
        float X = 0, Y = 0;
        if (side == 0)
        {
            X = x - 16.0f;
            Y = y + 20.0f;
        }
        else if (side == 1)
        {
            X = x + 16.0f;
            Y = y + 20.0f;
        }
        else if (side == 2)
        {
            X = x + 20.0f;
            Y = y - 12.5f;
        }
        else
        {
            X = x + 20.0f;
            Y = y + 18.0f;
        }
        float X1 = (x + X) / 2.0f;
        float Y1 = (y + Y) / 2.0f;
        return new Vector3(X1, Y1);
    }
}
