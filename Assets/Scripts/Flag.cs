using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    public GameObject guidelinePrefab;
    public Flag nextFlag;
    public bool isActive = false;
    public bool isTriggered = false;
    public bool isFinalFlag = false;
    public bool isFirstFlag = false;

    private PlayerController player;
    private SpriteRenderer sprite;
    private Animator animator;

    public bool isConditional;
    public GameObject conditional;
    private GameObject guideline;
    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
        if (isFirstFlag && !isConditional)
        {
            isActive = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (isConditional)
        {
            if (!conditional.activeInHierarchy && !isTriggered)
            {
                isActive = true;
            }
        }
        if (isActive)
        {
            sprite.sortingOrder = 1;
            animator.SetBool("IsActive", true);
        }
        else
        {
            sprite.sortingOrder = 0;
            animator.SetBool("IsActive", false);
        }
        if (!isTriggered && guideline != null)
        {
            Destroy(guideline);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (player.isBouncing && collision.tag == "Player" && !isTriggered && isActive)
        {
            isTriggered = true;
            isActive = false;
            FindObjectOfType<AudioManager>().Play("Healed");
            if (!isFinalFlag)
            {
                nextFlag.isActive = true;
                Point();
            }
        }
    }
    private void Point()
    {
        float length = Vector3.Distance(transform.position, nextFlag.transform.position);
        Vector3 v = (nextFlag.transform.position - transform.position);
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        Quaternion direction = Quaternion.Euler(0, 0, angle);
        guideline  = Instantiate(guidelinePrefab, Vector3.zero, Quaternion.identity);
        guideline.GetComponent<GuideLine>().Point(transform.position, direction, length);
    }

}
