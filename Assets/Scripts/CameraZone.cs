using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    public GameObject camera;
    private void Awake()
    {
        camera.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            camera.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            camera.SetActive(false);
        }
    }
}
