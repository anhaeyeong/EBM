using UnityEngine;
using UnityEngine.UI;

public class BulletMagnetism : MonoBehaviour
{
    public Image crosshair;
    public float magnetismRange = 50f; // 필요에 따라 범위 조정
    public LayerMask targetLayerMask; // 타겟 필터링에 사용할 레이어 마스크

    private GunController gunController;
    private Vector3 magneticVec;
    private Transform closestTarget; // 현재 가장 가까운 타겟
    private TobiiIntegrationExample tobiiIntegrationExample;

    private void Start()
    {
        gunController = FindObjectOfType<GunController>();
        if (gunController == null)
        {
            Debug.LogError("GunController 스크립트를 찾을 수 없습니다.");
        }

        tobiiIntegrationExample = FindObjectOfType<TobiiIntegrationExample>();
        if (tobiiIntegrationExample == null)
        {
            Debug.LogError("TobiiIntegrationExample 스크립트를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (gunController == null) return;

        // magneticVec을 지속적으로 업데이트
        SetMagneticVec();

        // GunController의 magneticVec 설정
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
        // AimAssist의 targetVec가 화면 정중앙이 아닌 경우 return
        if (gunController.isTargetSet && gunController.targetVec != Vector3.zero) return;

        // 조준점의 2D 좌표
        Vector2 crosshairPosition = crosshair.rectTransform.anchoredPosition;

        // 현재 타겟이 범위를 벗어났는지 확인
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

        // 가장 가까운 타겟 찾기
        closestTarget = GetClosestTarget(crosshairPosition);
        if (closestTarget != null)
        {
            // 타겟을 향한 벡터를 계산
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
