using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombingTurret : MonoBehaviour
{
    public float fireRate = 10f;
    public bool inRange;
    private float fireTimer;

    public PlayerController playerStats;
    public GameObject explosionPrefab;


    public PolygonCollider2D normalCollider;
    public BoxCollider2D bounceCollider;
    public GameObject explosion;

    // Start is called before the first frame update
    void Start()
    {
        inRange = false;
        fireTimer = 0;
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
        if (playerStats.isBouncing == true)
        {
            bounceCollider.enabled = true;
            normalCollider.enabled = false;
        }
        else
        {
            bounceCollider.enabled = false;
            normalCollider.enabled = true;
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player" && playerStats.isBouncing == true)
        {
            if (explosion != null)
            {
                explosion.SetActive(false);
            }
            
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
        Debug.Log("Pew");
        explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
    }
}
