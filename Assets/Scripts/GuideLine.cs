using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideLine : MonoBehaviour
{
    public float duration = 1f;
    public float width = 0.2f;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    public void Point(Vector3 startPosition, Quaternion angle, float length)
    {
        transform.position = startPosition;
        transform.rotation = angle;
        transform.GetChild(0).transform.localPosition = new Vector3(length / 2f, 0, 0);
        transform.GetChild(0).transform.localScale = new Vector3(length, width, 0);
    }
}
