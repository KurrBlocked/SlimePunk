using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    public float fireRate = 10f;
    public bool inRange;
    private float fireTimer;

    public PlayerController playerStats;
    public GameObject rocketPrefab;
    GameObject rocket;

    // Start is called before the first frame update
    void Start()
    {
        inRange = false;
        fireTimer = 0;
        playerStats = FindAnyObjectByType<PlayerController>();
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
    }
    private void Update()
    {

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player" && playerStats.isBouncing == true)
        {
            Destroy(gameObject);
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
        Vector2 direction = position - transform.position;
        if (rocket == null)
        {
            rocket = Instantiate(rocketPrefab, transform.position + new Vector3(0, 0.275f, 0), Quaternion.identity);
        }
        
    }
}
