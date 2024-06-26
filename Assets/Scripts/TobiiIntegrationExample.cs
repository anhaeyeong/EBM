using UnityEngine;
using UnityEngine.UI;
using Tobii.GameIntegration.Net;

public class TobiiIntegrationExample : MonoBehaviour
{
    private bool _isApiInitialized;
    private GazePoint _gazePoint;
    private HeadPose _headPose;
    public Image gazePointImage; // 시선 포인트를 시각화할 Image UI 요소
    public CircleDrawer gazeRangeCircle; // Gaze 범위를 시각화할 CircleDrawer 요소
    public float gazeRadius = 50f; // 시선 범위 반지름
    public LayerMask targetLayerMask; // 타겟 필터링에 사용할 레이어 마스크

    private GunController gunController;
    private float detectionTime;
    public Transform currentTarget { get; private set; }
    private Transform lastTarget;
    private float lastTargetLostTime;
    private const float minDetectionTime = 0.1f; // 최소 탐지 시간
    private const float maxDetectionTime = 5f; // 최대 탐지 시간
    private const float targetGracePeriod = 1f; // 타겟을 놓쳤다가 다시 찾거나 새로운 타겟을 탐지할 때의 유예 시간

    void Start()
    {
        try
        {
            Debug.Log("Setting application name for Tobii Game Integration API...");
            TobiiGameIntegrationApi.SetApplicationName("YourGameName");

            Debug.Log("Prelinking all functions for Tobii Game Integration API...");
            TobiiGameIntegrationApi.PrelinkAll(); // Prelink all functions to ensure they are correctly linked

            Debug.Log("Initializing Tobii Game Integration API...");

            TobiiGameIntegrationApi.TrackTracker("tobii-prp://IS5FF-100203350232");
            Debug.Log(TobiiGameIntegrationApi.GetTrackerInfo().Url);
            Debug.Log(TobiiGameIntegrationApi.IsTrackerConnected());
            Debug.Log(TobiiGameIntegrationApi.IsTrackerEnabled());

            _isApiInitialized = true;

            gunController = FindObjectOfType<GunController>();
            if (gunController == null)
            {
                Debug.LogError("GunController 스크립트를 찾을 수 없습니다.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception during API initialization: " + ex.Message);
            Debug.LogError("Stack Trace: " + ex.StackTrace);
        }
    }

    void Update()
    {
        // API 상태 업데이트
        TobiiGameIntegrationApi.Update();

        // 최신 시선 데이터 가져오기
        if (TobiiGameIntegrationApi.TryGetLatestGazePoint(out _gazePoint))
        {
            Debug.Log("Gaze Point - X: " + _gazePoint.X + ", Y: " + _gazePoint.Y);
            UpdateGazePointVisualization(_gazePoint);
            DetectTargetsInGazeRange(_gazePoint, gazeRangeCircle, gazeRadius);
        }
        else
        {
            // TryGetLatestGazePoint가 실패하더라도, 탐지된 타겟이 유지되고 있다면 detectionTime을 증가시킴
            if (currentTarget != null)
            {
                detectionTime += Time.deltaTime;
                detectionTime = Mathf.Clamp(detectionTime, 0f, maxDetectionTime);
            }
        }

        // 최신 머리 위치 데이터 가져오기
        if (TobiiGameIntegrationApi.TryGetLatestHeadPose(out _headPose))
        {
            Debug.Log("Head Pose - Position: (" + _headPose.Position.X + ", " + _headPose.Position.Y + ", " + _headPose.Position.Z + ")");
        }

        // 탐지된 시간 로그 출력
        if (currentTarget != null)
        {
            Debug.Log($"탐지 시간: {detectionTime}초");
        }
    }

    void UpdateGazePointVisualization(GazePoint gazePoint)
    {
        // Tobii GazePoint의 X, Y 값이 -1 ~ 1 범위이므로 이를 0 ~ Screen.width / Screen.height 범위로 변환
        float normalizedX = (gazePoint.X + 1f) / 2f * Screen.width;
        float normalizedY = (gazePoint.Y + 1f) / 2f * Screen.height;

        Vector2 screenPoint = new Vector2(normalizedX, normalizedY);

        // 필요에 따라 보정 계수를 적용할 수 있습니다.
        float correctionFactorX = 1f; // X축 보정 계수
        float correctionFactorY = 1f; // Y축 보정 계수

        screenPoint.x = Mathf.Clamp(screenPoint.x * correctionFactorX, 0, Screen.width);
        screenPoint.y = Mathf.Clamp(screenPoint.y * correctionFactorY, 0, Screen.height);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gazePointImage.rectTransform.parent as RectTransform,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        gazePointImage.rectTransform.localPosition = localPoint;

        // Gaze 범위 원의 위치 업데이트 (크기는 고정)
        gazeRangeCircle.rectTransform.localPosition = localPoint;
    }

    void DetectTargetsInGazeRange(GazePoint gazePoint, CircleDrawer circleDrawer, float radius)
    {
        // 시선 중심의 2D 좌표
        Vector2 gazePosition = new Vector2(
            (gazePoint.X + 1f) / 2f * Screen.width,
            (gazePoint.Y + 1f) / 2f * Screen.height
        );

        // 가장 가까운 타겟 찾기
        Transform closestTarget = GetClosestTarget(gazePosition, radius);
        bool isSameTarget = (closestTarget == currentTarget);
        bool isSameLastTarget = (closestTarget == lastTarget && Time.time - lastTargetLostTime <= targetGracePeriod);

        if (closestTarget != null)
        {
            if (!isSameTarget && !isSameLastTarget)
            {
                // 새로운 타겟을 찾은 경우
                currentTarget = closestTarget;
                detectionTime = 0f; // 새로운 타겟이 탐지되면 탐지 시간 초기화
            }
            else if (isSameLastTarget)
            {
                // 유예 시간 내에 동일한 타겟을 다시 찾은 경우
                currentTarget = lastTarget;
                detectionTime += Time.deltaTime; // 탐지 시간 유지
            }
            else
            {
                // 동일한 타겟을 계속 탐지 중인 경우
                detectionTime += Time.deltaTime;
            }

            detectionTime = Mathf.Clamp(detectionTime, 0f, maxDetectionTime); // 최대 탐지 시간을 5초로 고정
            Debug.Log($"탐지된 타겟: {closestTarget.name}, 탐지 시간: {detectionTime}");
        }
        else
        {
            if (currentTarget != null)
            {
                // 타겟을 놓친 시간을 기록
                lastTargetLostTime = Time.time;
                lastTarget = currentTarget;
            }
            currentTarget = null;
            Debug.Log("탐지된 타겟 없음");
        }
    }

    private Transform GetClosestTarget(Vector2 gazePosition, float radius)
    {
        Collider[] targets = Physics.OverlapSphere(Camera.main.transform.position, radius, targetLayerMask);
        Transform closestTarget = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider target in targets)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(target.transform.position);
            float distance = Vector2.Distance(new Vector2(screenPoint.x, screenPoint.y), gazePosition);
            if (distance < minDistance)
            {
                closestTarget = target.transform;
                minDistance = distance;
            }
        }
        return closestTarget;
    }

    void OnDestroy()
    {
        if (_isApiInitialized)
        {
            TobiiGameIntegrationApi.Shutdown();
        }
    }

    public float GetDetectionTime()
    {
        // 최소 탐지 시간보다 짧으면 0을 반환
        if (detectionTime < minDetectionTime)
        {
            return 0f;
        }
        return detectionTime; // 탐지 시간을 반환
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
}
