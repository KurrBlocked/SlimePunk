using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagDoor : MonoBehaviour
{
    public FlagManager flagManager;
    void Update()
    {
        if (!flagManager.flags[flagManager.flags.Length - 1].isActive && flagManager.flags[flagManager.flags.Length - 1].isTriggered)
        {
            FindObjectOfType<AudioManager>().Play("DoorOpen");
            gameObject.SetActive(false);
        }
    }
}
