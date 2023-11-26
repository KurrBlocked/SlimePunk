using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingDrone : MonoBehaviour
{
    public float fireRate = 10f;
    public bool inRange;
    private float fireTimer;

    public PlayerController playerStats;
    public GameObject bulletPrefab;
    private int currentTravelPoint;
    public Vector2[] travelPoints;
    public float flySpeed = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        currentTravelPoint = 0;
        inRange = false;
        fireTimer = 0;
        playerStats = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (inRange)
        {
            fireTimer += 0.1f;
        }
        else
        {
            fireTimer = 0f;
        }
        inRange = false;

        if (travelPoints.Length > 1f)
        {
            if (transform.position.x != travelPoints[currentTravelPoint].x)
            {
                float flyDirection = Mathf.Sign(transform.position.x - travelPoints[currentTravelPoint].x) * -1f;
                transform.position = new Vector2(transform.position.x + flySpeed * flyDirection, transform.position.y);
            }
            if (transform.position.y != travelPoints[currentTravelPoint].y)
            {
                float flyDirection = Mathf.Sign(transform.position.y - travelPoints[currentTravelPoint].y) * -1f;
                transform.position = new Vector2(transform.position.x, transform.position.y + flySpeed * flyDirection);
            }

            if (Mathf.Round(travelPoints[currentTravelPoint].x) == Mathf.Round(transform.position.x) && Mathf.Round(travelPoints[currentTravelPoint].y) == Mathf.Round(transform.position.y))
            {
                currentTravelPoint++;
                if (currentTravelPoint == travelPoints.Length)
                {
                    currentTravelPoint = 0;
                }
            }
        }
    }
    private void Update()
    {

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player" && playerStats.isBouncing == true)
        {
            Destroy(gameObject);
            FindObjectOfType<AudioManager>().Play("Break");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            inRange = true;
            if (fireTimer > fireRate)
            {
                fireTimer = 0;
                Fire(other.transform.position);
            }
        }
    }

    private void Fire(Vector3 position)
    {
        FindObjectOfType<AudioManager>().Play("Laser");
        Vector2 direction = position - transform.position;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetTarget(direction);
    }
}
