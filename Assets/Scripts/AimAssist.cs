using UnityEngine;
using UnityEngine.UI;

public class AimAssist : MonoBehaviour
{
    public Image crosshair; // �������� ��Ÿ���� �̹���
    public float magnetismRange = 50f; // Ÿ���� Ž���� ����
    public LayerMask targetLayerMask; // Ÿ�� ���͸��� ����� ���̾� ����ũ
    public float smoothingTime = 0.2f; // ������ �̵��� �ε巯���� �����ϴ� �ð�

    private GunController gunController;
    private Vector3 targetVec;
    private Vector3 currentCrosshairPosition;
    private Vector3 crosshairVelocity = Vector3.zero; // SmoothDamp�� �ӵ� ����
    private Transform currentTarget; // ���� Ÿ���� �����ϴ� ����

    private void Start()
    {
        // GunController ��ũ��Ʈ�� ����
        gunController = FindObjectOfType<GunController>();
        if (gunController == null)
        {
            Debug.LogError("GunController ��ũ��Ʈ�� ã�� �� �����ϴ�.");
        }

        currentCrosshairPosition = crosshair.rectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (gunController == null) return;

        // targetVec�� ���������� ������Ʈ
        SetTargetVec();

        // �������� �ε巴�� �̵�
        SmoothMoveCrosshair();

        // ���� Ÿ���� �ִ� ���, Ÿ���� Ž�� ������ ������� Ȯ��
        if (currentTarget != null)
        {
            CheckTargetOutOfRange();
        }
    }

    private void SetTargetVec()
    {
        // ȭ���� ���߾� ��ǥ
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        // ���� ����� Ÿ�� ã��
        Transform closestTarget = GetClosestTarget(screenCenter);
        if (closestTarget != null)
        {
            // ���� ��ǥ���� Ÿ���� ��ġ�� ȭ�� ��ǥ�� ��ȯ
            Vector3 screenPoint = gunController.m_cam.WorldToScreenPoint(closestTarget.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                crosshair.rectTransform,
                screenPoint,
                null, // Screen Space - Overlay ��忡���� null ���
                out Vector2 localPoint
            );

            // ���߾��� ���� ���� Ÿ���� �ִ��� Ȯ��
            if (Vector2.Distance(screenPoint, screenCenter) <= magnetismRange)
            {
                // Ÿ���� �����Ͽ� �������� Ÿ���� ���ϵ��� ��
                targetVec = closestTarget.position;
                MoveCrosshairToTarget(localPoint);
                currentTarget = closestTarget; // ���� Ÿ�� ����

                // GunController�� targetVec ����
                gunController.targetVec = targetVec;
                gunController.isTargetSet = true; // targetVec�� �����Ǿ����� ǥ��
            }
            else
            {
                ClearTarget();
            }
        }
        else
        {
            ClearTarget();
        }
    }

    private Transform GetClosestTarget(Vector2 screenCenter)
    {
        Collider[] targets = Physics.OverlapSphere(gunController.m_cam.transform.position, Mathf.Infinity, targetLayerMask);
        Transform closestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider target in targets)
        {
            Vector3 screenPoint = gunController.m_cam.WorldToScreenPoint(target.transform.position);
            float distance = Vector2.Distance(new Vector2(screenPoint.x, screenPoint.y), screenCenter);
            if (distance < minDistance)
            {
                closestTarget = target.transform;
                minDistance = distance;
            }
        }
        return closestTarget;
    }

    private void ClearTarget()
    {
        targetVec = Vector3.zero;
        currentTarget = null;
        MoveCrosshairToCenter(); // �������� �������� �̵�
        gunController.ResetTargetVec(); // targetVec�� �ʱ�ȭ
    }

    private void MoveCrosshairToTarget(Vector2 targetPosition)
    {
        // �������� Ÿ���� ��ġ�� �̵�
        currentCrosshairPosition = new Vector3(targetPosition.x, targetPosition.y, 0);
    }

    private void MoveCrosshairToCenter()
    {
        // �������� ȭ�� �߾����� �̵�
        currentCrosshairPosition = Vector3.zero;
    }

    private void SmoothMoveCrosshair()
    {
        // �������� ���� ��ġ�� ��ǥ ��ġ ���̸� �ε巴�� �̵�
        Vector3 currentAnchoredPosition = new Vector3(crosshair.rectTransform.anchoredPosition.x, crosshair.rectTransform.anchoredPosition.y, 0);
        Vector3 smoothedPosition = Vector3.SmoothDamp(currentAnchoredPosition, currentCrosshairPosition, ref crosshairVelocity, smoothingTime);
        crosshair.rectTransform.anchoredPosition = new Vector2(smoothedPosition.x, smoothedPosition.y);
    }

    private void CheckTargetOutOfRange()
    {
        // ���� Ÿ���� Ž�� ������ ������� Ȯ��
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector3 screenPoint = gunController.m_cam.WorldToScreenPoint(currentTarget.position);

        if (Vector2.Distance(new Vector2(screenPoint.x, screenPoint.y), screenCenter) > magnetismRange)
        {
            ClearTarget();
        }
    }
}
