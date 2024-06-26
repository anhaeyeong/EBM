using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float baseHomingForce = 20f;
    public float maxHomingForce = 100f;
    private Rigidbody rb;

    private Vector3 magneticVec;
    private bool hasMagneticVec = false;
    private float currentHomingForce;
    private TobiiIntegrationExample tobiiIntegrationExample;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentHomingForce = baseHomingForce;
    }

    private void Start()
    {
        tobiiIntegrationExample = FindObjectOfType<TobiiIntegrationExample>();
        if (tobiiIntegrationExample == null)
        {
            Debug.LogError("TobiiIntegrationExample ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    public void SetInitialDirection(Vector3 initialDirection)
    {
        rb.velocity = initialDirection * speed;
    }

    public void SetMagneticVec(Vector3 magneticVec)
    {
        this.magneticVec = magneticVec;
        hasMagneticVec = true;
    }

    private void FixedUpdate()
    {
        if (hasMagneticVec)
        {
            Vector3 direction = magneticVec.normalized;
            float detectionTime = tobiiIntegrationExample.GetDetectionTime();

            if (detectionTime < 1f)
            {
                currentHomingForce = baseHomingForce;
            }
            else
            {
                currentHomingForce = CalculateHomingForce(detectionTime);
            }

            Debug.Log("Current Homing Force: " + currentHomingForce);
            Vector3 forceDirection = direction * currentHomingForce;
            rb.AddForce(forceDirection, ForceMode.Acceleration);
        }
    }

    private float CalculateHomingForce(float detectionTime)
    {
        float L = maxHomingForce;
        float k = 10f; // ���� ����
        float x0 = 0.6f; // �߾��� ���� (1��~4�� ������ �߾���)

        float normalizedDetectionTime = Mathf.Clamp(detectionTime / 5f, 0f, 1f);
        float logisticValue = 1 / (1 + Mathf.Exp(-k * (normalizedDetectionTime - x0)));

        return Mathf.Lerp(baseHomingForce, maxHomingForce, logisticValue);
    }
}
