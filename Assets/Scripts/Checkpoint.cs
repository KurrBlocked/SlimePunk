using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool reached;
    // Start is called before the first frame update
    void Start()
    {
        reached = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        reached = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        reached = false;
    }
}
