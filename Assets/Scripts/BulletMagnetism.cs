using UnityEngine;
using UnityEngine.UI;

public class BulletMagnetism : MonoBehaviour
{
    public Image crosshair;
    public float magnetismRange = 50f; // �ʿ信 ���� ���� ����
    public LayerMask targetLayerMask; // Ÿ�� ���͸��� ����� ���̾� ����ũ

    private GunController gunController;
    private Vector3 magneticVec;
    private Transform closestTarget; // ���� ���� ����� Ÿ��
    private TobiiIntegrationExample tobiiIntegrationExample;

    private void Start()
    {
        gunController = FindObjectOfType<GunController>();
        if (gunController == null)
        {
            Debug.LogError("GunController ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }

        tobiiIntegrationExample = FindObjectOfType<TobiiIntegrationExample>();
        if (tobiiIntegrationExample == null)
        {
            Debug.LogError("TobiiIntegrationExample ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    private void Update()
    {
        if (gunController == null) return;

        // magneticVec�� ���������� ������Ʈ
        SetMagneticVec();

        // GunController�� magneticVec ����
        if (closestTarget != null && closestTarget == tobiiIntegrationExample.GetCurrentTarget())
        {
            gunController.magneticVec = magneticVec;
            gunController.isTargetSet = magneticVec != Vector3.zero;
        }
        else
        {
            gunController.magneticVec = Vector3.zero;
            gunController.isTargetSet = false;
        }
    }

    private void SetMagneticVec()
    {
        // AimAssist�� targetVec�� ȭ�� ���߾��� �ƴ� ��� return
        if (gunController.isTargetSet && gunController.targetVec != Vector3.zero) return;

        // �������� 2D ��ǥ
        Vector2 crosshairPosition = crosshair.rectTransform.anchoredPosition;

        // ���� Ÿ���� ������ ������� Ȯ��
        if (closestTarget != null)
        {
            Vector3 screenPoint = gunController.m_cam.WorldToScreenPoint(closestTarget.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                crosshair.rectTransform,
                screenPoint,
                null,
                out Vector2 localPoint
            );

            if (Vector2.Distance(localPoint, crosshairPosition) > magnetismRange)
            {
                closestTarget = null;
                magneticVec = Vector3.zero;
            }
        }

        // ���� ����� Ÿ�� ã��
        closestTarget = GetClosestTarget(crosshairPosition);
        if (closestTarget != null)
        {
            // Ÿ���� ���� ���͸� ���
            Vector3 targetPosition = closestTarget.position;
            magneticVec = targetPosition - gunController.firepoint.position;
        }
        else
        {
            magneticVec = Vector3.zero;
        }
    }

    private Transform GetClosestTarget(Vector2 crosshairPosition)
    {
        Collider[] targets = Physics.OverlapSphere(gunController.m_cam.transform.position, Mathf.Infinity, targetLayerMask);
        Transform closestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider target in targets)
        {
            Vector3 screenPoint = gunController.m_cam.WorldToScreenPoint(target.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                crosshair.rectTransform,
                screenPoint,
                null,
                out Vector2 localPoint
            );

            float distance = Vector2.Distance(localPoint, crosshairPosition);
            if (distance < minDistance && distance <= magnetismRange)
            {
                closestTarget = target.transform;
                minDistance = distance;
            }
        }
        return closestTarget;
    }
}
