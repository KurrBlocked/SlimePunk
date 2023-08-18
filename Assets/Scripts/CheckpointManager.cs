using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    // Start is called before the first frame update'
    public GameObject[] checkpoints;
    public int lastReachedCheckpoint;
    void Start()
    {
        //Initialize with an invalid checkpoint
        lastReachedCheckpoint = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (checkpoints.Length > 0)
        {
            for (int i = 0; i < checkpoints.Length; i++)
            {
                if (checkpoints[i].GetComponent<Checkpoint>().reached)
                {
                    lastReachedCheckpoint = i;
                }
            }
        }
    }
    public Vector2 returnLastReachedCheckpointPosition()
    {
        return checkpoints[lastReachedCheckpoint].transform.position;
    }
}
