using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{

    private float rightMax = 5.0f;
    private float leftMax = -5.0f;
    private Vector3 currentPos;
    private bool moveRight = true;
    // Start is called before the first frame update
    void Start()
    {
        currentPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        currentPos = transform.position;
        checkDirection();
        if(moveRight)
        {
            transform.Translate(Vector3.right * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.left * Time.deltaTime);
        }
    }

    private void checkDirection()
    {
        if(currentPos.x >= rightMax)
        {
            moveRight = false;
        }
        if(currentPos.x <= leftMax)
        {
            moveRight = true;
        }
    }
}
