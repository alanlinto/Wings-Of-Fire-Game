using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public Transform firingPoint;
    public GameObject bulletPrefab, parent;
    float timer;
    bool setInput;
    Camera mainCamera;
    Vector3 mousePos;

    PlayerStats stats;

    // Start is called before the first frame update
    void Start()
    {
        this.mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        this.timer = 0.0f;
        this.setInput = false;
        stats = GetComponentInParent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        this.setRotation();

        if (Input.GetMouseButton(0) && stats.IsAlive())
        {
            this.setInput = true;
        }

        this.bulletSpawnTimer();
    }

    void setRotation()
    {
        //Calculating the Direction/Unit vector for the bullet to aim and shoot
        this.mousePos = this.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rotatPoint = this.mousePos - transform.position;
        float rotatAngle = Mathf.Atan2(rotatPoint.y, rotatPoint.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotatAngle);
    }
    void bulletSpawnTimer()
    {
        if (this.setInput == true)
        {
            if (this.timer >= 0.3f)
            {
                this.timer = 0.0f;
                this.setInput = false;
                Instantiate(bulletPrefab, firingPoint.position, transform.rotation, parent.transform);
            }
            else
            {
                this.timer += Time.deltaTime;
            }
        }
    }

}
