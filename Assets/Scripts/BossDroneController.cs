using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDroneController : MonoBehaviour
{
    public GameObject activator;
    private PlayerController player;
    public Animator animator;

    public bool goRight;
    public bool activated;
    public bool ready;
    public Vector2 startPosition;
    public float flySpeed;
    public int phase;
    public int health = 30;


    private float phaseTimer;
    public float phase1Duration = 20f;
    public int phase2HealthRequirement = 22;
    public float phase2Duration = 20f;
    public int phase3HealthRequirement = 15;
    public int phase3SpeedMultiplier = 4;
    public float pauseTimeBetweenCharges = 1f;
    public float pauseBeforePhase1 = 1f;
    public float pauseTracker;
    public float bootUpTime = 4f;
    private float bootUpTimer;
    private bool isCurrentPhaseFinished;
    private float waitTimerP3;
    public float PauseBeforePhase3 = 3f;

    //Shooting
    public GameObject bulletPrefab;
    private float laserFireTimer;
    public float phase0FireRate = 5f;
    public float phase1FireRate = 1f;

    //Bombing
    public GameObject explosionPrefab;
    private float bombingTimer;
    public float bombingFireRate = 5f;

    //Rockets
    public GameObject rocketPrefab;
    private float rocketTimer;
    public float rocketFireRate = 9f;
    public Vector2 rocketLaunchingPosition;

    public bool isKnivesOut;
    public PolygonCollider2D knivesCollider;
    public PolygonCollider2D baseCollider;


    public Vector2[] phase0TravelPoints;
    public int currentP0TravelPoint;
    public Vector2[] phase3TravelPointsLeft;
    public Vector2[] phase3TravelPointsRight;
    private int currentP3TravelPoint;

    //Testing

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        bootUpTimer = 0f;
        phaseTimer = 0f;
        activated = false;
        ready = false;
        isCurrentPhaseFinished = false;
        phase = -1;
        player = FindAnyObjectByType<PlayerController>();
    }
    void Update()
    {
        if (activator.GetComponent<BossDroneActivator>().activated)
        {
            activated = true;
        }
        if (isKnivesOut)
        {
            animator.SetBool("KnivesOut", true);
            gameObject.tag = "Hazard";
            baseCollider.enabled = false;
            knivesCollider.enabled = true;
        }
        else
        {
            baseCollider.enabled = true;
            knivesCollider.enabled = false;
            animator.SetBool("KnivesOut", false);
            gameObject.tag = "Untagged";
        }
    }
    private void FixedUpdate()
    {
        if (health <= 0)
        {
            ready = false;
            gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
        else if (activated && !ready)
        {
            StartUp();
        }
        if (ready)
        {
            if (isCurrentPhaseFinished)
            {
                PhaseSwitch();
            }
            else
            {
                switch (phase)
                {
                    case 0:
                        //In-between sentry mode phase
                        Phase0();
                        break;
                    case 1:
                        //Shoot and PA-Bombing
                        Phase1();
                        break;
                    case 2:
                        //Rocket barage
                        Phase2();
                        break;
                    case 3:
                        //Charge with knives and rockets
                        Phase3();
                        break;
                    default:
                        Debug.Log("Unknown Phase");
                        break;
                }
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            if (player.isBouncing)
            {
                health--;
                FindObjectOfType<AudioManager>().Play("ElectricHit");
                if (health == 0)
                {
                    FindObjectOfType<AudioManager>().Play("Break");
                }
                if (health == phase2HealthRequirement || health == phase3HealthRequirement)
                {
                    FindObjectOfType<AudioManager>().Play("PhaseChange");
                }
            }
        }
    }
    void StartUp()
    {
        if (!(Mathf.Abs(transform.position.y - startPosition.y) < 0.001f))
        {
            float flyDirection = Mathf.Sign(transform.position.y - startPosition.y) * -1f;
            transform.position = new Vector2(transform.position.x, transform.position.y + flySpeed * flyDirection);
        }
        else if (!(Mathf.Abs(transform.position.x - startPosition.x) < 0.001f))
        {
            float flyDirection = Mathf.Sign(transform.position.x - startPosition.x) * -1f;
            transform.position = new Vector2(transform.position.x + flySpeed * flyDirection, transform.position.y);
        }
        else
        {
            bootUpTimer += 0.1f;
            if (bootUpTime < bootUpTimer)
            {
                ready = true;
                isCurrentPhaseFinished = true;
                FindObjectOfType<AudioManager>().Play("BootUp");
            }
        }
    }
    void PhaseSwitch()
    {
        //Decides phase to switch to based on health and reset certain variables
        if (phase == 0)
        {
            phaseTimer = 0f;
            if (health <= phase3HealthRequirement)
            {
                phase = 3;
                currentP3TravelPoint = 0;
                waitTimerP3 = 0;
                if (player.transform.position.x < -67f)
                {
                    goRight = false;
                }
                else
                {
                    goRight = true;
                }
                FindObjectOfType<AudioManager>().Play("KnivesOut");
                isKnivesOut = true;
            }
            else if (health <= phase2HealthRequirement)
            {
                phase = 2;
                rocketTimer = rocketFireRate / 1.5f;
            }
            else
            {
                phase = 1;
                laserFireTimer = 0;
                bombingTimer = 0;
                FindObjectOfType<AudioManager>().Play("TurretPrimed");
            }
        }
        else 
        {
            phase = 0;
            currentP0TravelPoint = 0;
            
        }
        pauseTracker = 0;
        isCurrentPhaseFinished = false;
        transform.position = startPosition;
    }
    void Phase0()
    {
        Move(phase0TravelPoints, currentP0TravelPoint);
        if (Mathf.Round(phase0TravelPoints[currentP0TravelPoint].x) == Mathf.Round(transform.position.x) && Mathf.Round(phase0TravelPoints[currentP0TravelPoint].y) == Mathf.Round(transform.position.y))
        {
            currentP0TravelPoint++;
        }
        laserFireTimer += 0.1f;
        if (laserFireTimer > phase0FireRate)
        {
            laserFireTimer = 0;
            FireLaser(player.transform.position);
        }
        if (currentP0TravelPoint == phase0TravelPoints.Length)
        {
            isCurrentPhaseFinished = true;
        }
    }
    void Phase1()
    {
        if (pauseTracker > pauseBeforePhase1)
        {
            //Firing
            laserFireTimer += 0.1f;
            if (laserFireTimer > phase1FireRate)
            {
                laserFireTimer = 0;
                FireLaser(player.transform.position);
            }
            //Bombing
            bombingTimer += 0.1f;
            if (bombingTimer > bombingFireRate)
            {
                bombingTimer = 0;
                LockOnBomb(player.transform.position);
            }
            phaseTimer += 0.1f;
            if (phaseTimer > phase1Duration)
            {
                isCurrentPhaseFinished = true;
            }
        }
        else
        {
            pauseTracker += 0.1f;
        }
    }
    void Phase2()
    {
        if (phaseTimer > phase2Duration)
        {
            if (Mathf.Round(startPosition.x) == Mathf.Round(transform.position.x) && Mathf.Round(startPosition.y) == Mathf.Round(transform.position.y))
            {
                isCurrentPhaseFinished = true;
            }
            else
            {
                Move(new Vector2[] { startPosition }, 0);
            }
        }
        else if (Mathf.Round(rocketLaunchingPosition.x) == Mathf.Round(transform.position.x) && Mathf.Round(rocketLaunchingPosition.y) == Mathf.Round(transform.position.y))
        {
            phaseTimer += 0.1f;
            rocketTimer += 0.1f;
            if (rocketTimer > rocketFireRate)
            {
                rocketTimer = 0;
                LaunchRockets();
            }
        }
        else
        {
            Move(new Vector2[] { rocketLaunchingPosition }, 0);
            if (Mathf.Round(rocketLaunchingPosition.x) == Mathf.Round(transform.position.x) && Mathf.Round(rocketLaunchingPosition.y) == Mathf.Round(transform.position.y))
            {
                FindObjectOfType<AudioManager>().Play("BomberPrimed");
            }
        }
    }
    void Phase3()
    {
        if (waitTimerP3 > PauseBeforePhase3)
        {
            if (goRight)
            {
                Move(phase3TravelPointsRight, currentP3TravelPoint);
                if (Mathf.Round(phase3TravelPointsRight[currentP3TravelPoint].x) == Mathf.Round(transform.position.x) && Mathf.Round(phase3TravelPointsRight[currentP3TravelPoint].y) == Mathf.Round(transform.position.y))
                {
                    pauseTracker += 0.1f;
                    if (pauseTracker > pauseTimeBetweenCharges)
                    {
                        currentP3TravelPoint++;
                        pauseTracker = 0f;
                        if (currentP3TravelPoint % 2 == 1)
                        {
                            LaunchSingleRocket(player.transform.position);
                        }
                    }
                }
                if (currentP3TravelPoint == phase3TravelPointsRight.Length)
                {
                    isCurrentPhaseFinished = true;
                    isKnivesOut = false;
                }
            }
            else
            {
                Move(phase3TravelPointsLeft, currentP3TravelPoint);
                if (Mathf.Round(phase3TravelPointsLeft[currentP3TravelPoint].x) == Mathf.Round(transform.position.x) && Mathf.Round(phase3TravelPointsLeft[currentP3TravelPoint].y) == Mathf.Round(transform.position.y))
                {
                    pauseTracker += 0.1f;
                    if (pauseTracker > pauseTimeBetweenCharges)
                    {
                        currentP3TravelPoint++;
                        pauseTracker = 0f;
                        if (currentP3TravelPoint % 2 == 1)
                        {
                            LaunchSingleRocket(player.transform.position);
                        }
                    }
                }
                if (currentP3TravelPoint == phase3TravelPointsLeft.Length)
                {
                    isCurrentPhaseFinished = true;
                    isKnivesOut = false;
                }
            }
        }
        else
        {
            waitTimerP3 += 0.1f;
        }
    }
    void Move(Vector2[] travelPoints, int currentTravelPoint) 
    {
        if (transform.position.x != travelPoints[currentTravelPoint].x)
        {
            float flyDirection = Mathf.Sign(transform.position.x - travelPoints[currentTravelPoint].x) * -1f;
            if (phase == 3)
            {
                transform.position = new Vector2(transform.position.x + flySpeed * phase3SpeedMultiplier * flyDirection, transform.position.y);
            }
            else
            {
                transform.position = new Vector2(transform.position.x + flySpeed * flyDirection, transform.position.y);
            }
        }
        if (transform.position.y != travelPoints[currentTravelPoint].y)
        {
            float flyDirection = Mathf.Sign(transform.position.y - travelPoints[currentTravelPoint].y) * -1f;
            if (phase == 3)
            {
                transform.position = new Vector2(transform.position.x, transform.position.y + flySpeed * phase3SpeedMultiplier * flyDirection);
            }
            else
            {
                transform.position = new Vector2(transform.position.x, transform.position.y + flySpeed * flyDirection);
            }
        }
    }
    private void FireLaser(Vector3 position)
    {
        FindObjectOfType<AudioManager>().Play("Laser");
        Vector2 direction = position - transform.position;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetTarget(direction);
    }
    private void LockOnBomb(Vector3 position)
    {
        Instantiate(explosionPrefab, position, Quaternion.identity);
    }
    private void LaunchRockets()
    {
        if (player.transform.position.x < -67f)
        {
            Instantiate(rocketPrefab, transform.position + new Vector3(0, 0.275f, 0), Quaternion.Euler(0f, 0f, 0f));
            Instantiate(rocketPrefab, transform.position + new Vector3(0, 0.275f, 0), Quaternion.Euler(0f, 0f, 225));
        }
        else
        {
            Instantiate(rocketPrefab, transform.position + new Vector3(0, 0.275f, 0), Quaternion.Euler(0f, 0f, 180f));
            Instantiate(rocketPrefab, transform.position + new Vector3(0, 0.275f, 0), Quaternion.Euler(0f, 0f, 315));
        }
        Instantiate(rocketPrefab, transform.position + new Vector3(0, 0.275f, 0), Quaternion.Euler(0f, 0f, 270f));
    }
    private void LaunchSingleRocket(Vector3 position)
    {
        Vector2 direction = position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += 180f;
        Instantiate(rocketPrefab, transform.position + new Vector3(0, 0.275f, 0), Quaternion.Euler(0f, 0f, angle));
    }
}
