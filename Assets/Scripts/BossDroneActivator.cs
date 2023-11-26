using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDroneActivator : MonoBehaviour
{
    public bool activated;
    private BoxCollider2D col;
    // Start is called before the first frame update
    void Start()
    {
        activated = false;
        col = gameObject.GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            activated = true;
            col.enabled = false;
        }
    }
}
