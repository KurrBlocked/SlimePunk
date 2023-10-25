using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombingTurretExplosion : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite aimSpot;
    public Sprite explosion;
    public float duration = 4f;
    public float timer;


    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        spriteRenderer.sprite = aimSpot;
        Destroy(gameObject, duration);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > (duration - 0.14f))
        {
            spriteRenderer.sprite = explosion;
            gameObject.tag = "Hazard";
        }
    }
}
