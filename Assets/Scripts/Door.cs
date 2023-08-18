using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject[] keys;
    public bool opened;
    // Start is called before the first frame update
    void Start()
    {
        opened = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!opened) 
        {
            opened = true;
            foreach (GameObject key in keys)
            {
                if (key.activeInHierarchy)
                {
                    opened = false;
                }
            }
        }
        if (opened)
        {
            gameObject.SetActive(false);
        }
    }
}
