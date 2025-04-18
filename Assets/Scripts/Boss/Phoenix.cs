using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phoenix : MonoBehaviour
{
    public GameObject boss;

    void Update()
    {
        transform.RotateAround(boss.transform.position, new Vector3(0,0,1), Time.deltaTime * 50f);

        GetComponent<SpriteRenderer>().flipY = transform.rotation.z > 0 ? true : false;
    }
}
