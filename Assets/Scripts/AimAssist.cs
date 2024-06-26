using UnityEngine;
using UnityEngine.UI;

public class AimAssist : MonoBehaviour
{
    public Image crosshair; // 조준점을 나타내는 이미지
    public float magnetismRange = 50f; // 타겟을 탐지할 범위
    public LayerMask targetLayerMask; // 타겟 필터링에 사용할 레이어 마스크
    public float smoothingTime = 0.2f; // 조준점 이동의 부드러움을 조절하는 시간

    private GunController gunController;
    private Vector3 targetVec;
    private Vector3 currentCrosshairPosition;
    private Vector3 crosshairVelocity = Vector3.zero; // SmoothDamp의 속도 벡터
    private Transform currentTarget; // 현재 타겟을 저장하는 변수

    private void Start()
    {
        // GunController 스크립트에 접근
        gunController = FindObjectOfType<GunController>();
        if (gunController == null)
        {
            Debug.LogError("GunController 스크립트를 찾을 수 없습니다.");
        }

        currentCrosshairPosition = crosshair.rectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (gunController == null) return;

        // targetVec을 지속적으로 업데이트
        SetTargetVec();

        // 조준점을 부드럽게 이동
        SmoothMoveCrosshair();

        // 현재 타겟이 있는 경우, 타겟이 탐지 범위를 벗어났는지 확인
        if (currentTarget != null)
        {
            CheckTargetOutOfRange();
        }
    }

    private void SetTargetVec()
    {
        // 화면의 정중앙 좌표
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        // 가장 가까운 타겟 찾기
        Transform closestTarget = GetClosestTarget(screenCenter);
        if (closestTarget != null)
        {
            // 월드 좌표에서 타겟의 위치를 화면 좌표로 변환
            Vector3 screenPoint = gunController.m_cam.WorldToScreenPoint(closestTarget.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                crosshair.rectTransform,
                screenPoint,
                null, // Screen Space - Overlay 모드에서는 null 사용
                out Vector2 localPoint
            );

            // 정중앙의 범위 내에 타겟이 있는지 확인
            if (Vector2.Distance(screenPoint, screenCenter) <= magnetismRange)
            {
                // 타겟을 설정하여 조준점이 타겟을 향하도록 함
                targetVec = closestTarget.position;
                MoveCrosshairToTarget(localPoint);
                currentTarget = closestTarget; // 현재 타겟 설정

                // GunController의 targetVec 설정
                gunController.targetVec = targetVec;
                gunController.isTargetSet = true; // targetVec이 설정되었음을 표시
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
        MoveCrosshairToCenter(); // 조준점을 원점으로 이동
        gunController.ResetTargetVec(); // targetVec을 초기화
    }

    private void MoveCrosshairToTarget(Vector2 targetPosition)
    {
        // 조준점을 타겟의 위치로 이동
        currentCrosshairPosition = new Vector3(targetPosition.x, targetPosition.y, 0);
    }

    private void MoveCrosshairToCenter()
    {
        // 조준점을 화면 중앙으로 이동
        currentCrosshairPosition = Vector3.zero;
    }

    private void SmoothMoveCrosshair()
    {
        // 조준점의 현재 위치와 목표 위치 사이를 부드럽게 이동
        Vector3 currentAnchoredPosition = new Vector3(crosshair.rectTransform.anchoredPosition.x, crosshair.rectTransform.anchoredPosition.y, 0);
        Vector3 smoothedPosition = Vector3.SmoothDamp(currentAnchoredPosition, currentCrosshairPosition, ref crosshairVelocity, smoothingTime);
        crosshair.rectTransform.anchoredPosition = new Vector2(smoothedPosition.x, smoothedPosition.y);
    }

    private void CheckTargetOutOfRange()
    {
        // 현재 타겟이 탐지 범위를 벗어났는지 확인
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector3 screenPoint = gunController.m_cam.WorldToScreenPoint(currentTarget.position);

        if (Vector2.Distance(new Vector2(screenPoint.x, screenPoint.y), screenCenter) > magnetismRange)
        {
            ClearTarget();
        }
    }
}
