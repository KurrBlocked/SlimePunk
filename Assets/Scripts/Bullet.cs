using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float duration = 4f;

    private Vector2 targetDirection;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        transform.Translate(targetDirection * speed * Time.deltaTime);
    }

    public void SetTarget(Vector2 direction)
    {
        targetDirection = direction.normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        if (collision.CompareTag("Player") || collision.CompareTag("WAF"))
        {
            Destroy(gameObject, 0.02f);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.name);
    }
}