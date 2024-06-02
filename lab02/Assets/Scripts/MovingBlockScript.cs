using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MovingBlockScript : MonoBehaviour
{
    public float shiftZ;
    public float speed = 1f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool forward = true;
    private float eps = 0.01f;

    private void Awake()
    {
        startPosition = transform.position;
        endPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z + shiftZ);
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        if (forward)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPosition, step);
            if (Vector3.Distance(transform.position, endPosition) < eps)
                forward = false;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, step);
            if (Vector3.Distance(transform.position, startPosition) < eps)
                forward = true;
        }
    }
}
