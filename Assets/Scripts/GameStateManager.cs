using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.healthCount == 0)
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "Test":
                    SceneManager.LoadScene("Test");
                    break;
                default:
                    Debug.Log("Unknown level");
                    break;
            }
        }
    }
}
