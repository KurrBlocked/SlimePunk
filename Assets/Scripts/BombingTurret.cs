using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombingTurret : MonoBehaviour
{
    public float fireRate = 20f;
    public bool inRange;
    private float fireTimer;

    public float bombingInterval = 5f;
    private float bombingTimer;

    public PlayerController playerStats;
    

    public PolygonCollider2D normalCollider;
    public BoxCollider2D bounceCollider;

    private int bombCount;
    public int maxBombCount = 3;
    private bool  isLoaded;

    public GameObject explosionPrefab;

    public GameObject[] explosions;

    // Start is called before the first frame update
    void Start()
    {
        inRange = false;
        fireTimer = 0;
        bombCount = 0;
        isLoaded = false;
        explosions = new GameObject[maxBombCount];
        playerStats = FindAnyObjectByType<PlayerController>();

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (isLoaded)
        {
            if (inRange)
            {
                bombingTimer += 0.1f;
            }
            else
            {
                bombingTimer = 0f;
                isLoaded = false;
            }
            if (bombingTimer > bombingInterval)
            {
                bombingTimer = 0f;
                if (bombCount > 0)
                {
                    bombCount--;
                    Fire(playerStats.transform.position);
                }
                else
                {
                    isLoaded = false;
                }
            }
        }
        else
        {
            if (fireTimer > (fireRate * 0.9f) && fireTimer < (fireRate * 0.9f) + 0.1f)
            {
                FindObjectOfType<AudioManager>().Play("BomberPrimed");
            }
            if (inRange)
            {
                fireTimer += 0.1f;
            }
            else
            {
                fireTimer = 0f;
            }
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
            foreach (GameObject exp in explosions)
            {
                if (exp != null)
                {
                    exp.SetActive(false);
                }
            }
            Destroy(gameObject);
            FindObjectOfType<AudioManager>().Play("Break");
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            inRange = true;
            if (fireTimer > fireRate && !isLoaded)
            {
                fireTimer = 0;
                isLoaded = true;
                bombCount = maxBombCount;
            }
        }
    }
    private void Fire(Vector3 position)
    {
        explosions[bombCount] = Instantiate(explosionPrefab, position, Quaternion.identity);
    }
}
