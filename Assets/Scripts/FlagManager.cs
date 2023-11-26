using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    private PlayerController player;
    public Flag[] flags;
    // Start is called before the first frame update
    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !player.isBouncing && player.respawnTimer <= 0 && flags[0].isTriggered && !flags[flags.Length - 1].isTriggered)
        {
            player.Respawn();
            foreach (Flag f in flags)
            {
                f.isTriggered = false;
                f.isActive = false;
            }
            flags[0].isActive = true;
        }
    }

}
