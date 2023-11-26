using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public PlayerController player;
    public Animator transition;
    public float transitionTime = 1f;
    public float respawnTransitionTime = 2f;
    public bool respawning;
    public bool isPlayableScene = true;
    public bool tutorialLevel = false;
    public bool isPaused;
    public GameObject pauseMenu;
    private CanvasGroup pauseScreen;
    public bool bossScene = false;
    public BossDroneController drone;
    private bool triggerEnding;
    public DialogueTrigger finalDialogue;

    // Start is called before the first frame update
    void Start()
    {
        triggerEnding = false;
        if (isPlayableScene)
        {
            isPaused = false;
            player = FindAnyObjectByType<PlayerController>();
            respawning = false;
            pauseScreen = pauseMenu.GetComponent<CanvasGroup>();
        }
        if (bossScene)
        {
            drone = FindAnyObjectByType<BossDroneController>();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (isPlayableScene)
        {
            Time.timeScale = 1;
            pauseScreen.alpha = 0;
            pauseScreen.interactable = false;
            pauseScreen.blocksRaycasts = false;
            if (player.healthCount <= 0 && !triggerEnding)
            {
                respawning = true;
                if (SceneManager.GetActiveScene().name == "Boss1")
                {
                    StartCoroutine(LoadScene("RespawnAtBoss1"));
                }
                else
                {
                    StartCoroutine(LoadScene(SceneManager.GetActiveScene().name));
                }
            }
            else
            {
                if (player.pausePressedInput)
                {
                    isPaused = !isPaused;
                }
                if (isPaused)
                {
                    Time.timeScale = 0;
                    pauseScreen.alpha = 1;
                    pauseScreen.interactable = true;
                    pauseScreen.blocksRaycasts = true;
                }
                if (bossScene)
                {
                    if (drone.health <= 0)
                    {
                        triggerEnding = true;
                    }
                }
                if (player.restartLevelInput)
                {
                    if (tutorialLevel)
                    {
                        player.Respawn();
                    }
                    else
                    {
                        StartCoroutine(LoadScene(SceneManager.GetActiveScene().name));
                    }
                }
            }

            if (finalDialogue != null)
            {
                if (finalDialogue.dialogueIsFinished)
                {
                    player.healthCount = 0;
                    player.freezePlayerMovement = true;
                    if (!player.isDead)
                    {
                        FindObjectOfType<AudioManager>().Play("BootUp");
                    }
                    player.isDead = true;
                }
                if (player.isDead)
                {
                    StartCoroutine(LoadScene("EndScreen", 5f));
                }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "Tutorial":
                    StartCoroutine(LoadScene("StartMenu"));
                    break;
                case "PreRun":
                    StartCoroutine(LoadScene("Showcase1"));
                    break;
                case "Showcase1":
                    StartCoroutine(LoadScene("Showcase2"));
                    break;
                case "Showcase2":
                    StartCoroutine(LoadScene("Boss1"));
                    break;
                default:
                    Debug.Log("Unknown level");
                    break;
            }
        }
    }
    public void LoadStart()
    {
        SceneManager.LoadScene("StartMenu");
    }
    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }
    public void LoadInstruction()
    {
        SceneManager.LoadScene("Instruction");
    }
    public void LoadGame()
    {
        SceneManager.LoadScene("PreRun");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator LoadScene(string name)
    {
        if (respawning)
        {
            yield return new WaitForSeconds(respawnTransitionTime);
        }
        transition.SetBool("Start", true);
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(name);
    }
    IEnumerator LoadScene(string name, float time)
    {
        yield return new WaitForSeconds(time);
        transition.SetBool("Start", true);
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(name);
    }
}
