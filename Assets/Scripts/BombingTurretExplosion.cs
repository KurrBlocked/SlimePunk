using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombingTurretExplosion : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite aimSpot;
    public Sprite explosion;
    public float duration = 4f;
    public float explosionDuration = 0.10f;
    public float timer;


    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        FindObjectOfType<AudioManager>().Play("ExplosionPrimed");
        spriteRenderer.sprite = aimSpot;
        Destroy(gameObject, duration);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > (duration - explosionDuration))
        {
            FindObjectOfType<AudioManager>().Play("SmallExplosion");
            spriteRenderer.sprite = explosion;
            gameObject.tag = "Hazard";
        }
    }
}
