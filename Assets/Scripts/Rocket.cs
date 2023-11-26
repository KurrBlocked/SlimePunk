using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Transform playerLocation;
    public Sprite rocket;
    public Sprite explosion;
    public float duration = 0.1f;
    public float missileSpeed = 0.1f;
    public float speedTurnReductionRate = 0.9f;
    public float rotationSpeed = 100.0f;
    public float maxVelocity = 50f;

    private CircleCollider2D explosionCollider;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer.sprite = rocket;
        playerLocation = GameObject.Find("Player").transform;
        explosionCollider = GetComponent<CircleCollider2D>();
        explosionCollider.enabled = false;
        FindObjectOfType<AudioManager>().Play("RocketLaunched");
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (gameObject.tag != "Hazard")
        {
            Vector3 direction = playerLocation.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle += 180f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        

        Vector2 forceDirection = Quaternion.Euler(0, 0, transform.eulerAngles.z) * Vector2.left;
        rb.AddForce(forceDirection * missileSpeed, ForceMode2D.Impulse);
        float dotProduct = Vector2.Dot(rb.velocity.normalized, forceDirection.normalized);
        if (dotProduct < 0.8)
        {
            rb.velocity *= speedTurnReductionRate;
        }

        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("WAF"))
        {
            FindObjectOfType<AudioManager>().Play("BigExplosion");
            spriteRenderer.sprite = explosion;
            gameObject.tag = "Hazard";
            Destroy(gameObject, duration);
            transform.rotation = Quaternion.Euler(0,0,0);
            rb.constraints = rb.constraints | RigidbodyConstraints2D.FreezePosition;
            explosionCollider.enabled = true;
        }
    }
}