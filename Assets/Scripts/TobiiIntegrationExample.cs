using UnityEngine;
using UnityEngine.UI;
using Tobii.GameIntegration.Net;

public class TobiiIntegrationExample : MonoBehaviour
{
    private bool _isApiInitialized;
    private GazePoint _gazePoint;
    private HeadPose _headPose;
    public Image gazePointImage; // �ü� ����Ʈ�� �ð�ȭ�� Image UI ���
    public CircleDrawer gazeRangeCircle; // Gaze ������ �ð�ȭ�� CircleDrawer ���
    public float gazeRadius = 50f; // �ü� ���� ������
    public LayerMask targetLayerMask; // Ÿ�� ���͸��� ����� ���̾� ����ũ

    private GunController gunController;
    private float detectionTime;
    public Transform currentTarget { get; private set; }
    private Transform lastTarget;
    private float lastTargetLostTime;
    private const float minDetectionTime = 0.1f; // �ּ� Ž�� �ð�
    private const float maxDetectionTime = 5f; // �ִ� Ž�� �ð�
    private const float targetGracePeriod = 1f; // Ÿ���� ���ƴٰ� �ٽ� ã�ų� ���ο� Ÿ���� Ž���� ���� ���� �ð�

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
                Debug.LogError("GunController ��ũ��Ʈ�� ã�� �� �����ϴ�.");
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
        // API ���� ������Ʈ
        TobiiGameIntegrationApi.Update();

        // �ֽ� �ü� ������ ��������
        if (TobiiGameIntegrationApi.TryGetLatestGazePoint(out _gazePoint))
        {
            Debug.Log("Gaze Point - X: " + _gazePoint.X + ", Y: " + _gazePoint.Y);
            UpdateGazePointVisualization(_gazePoint);
            DetectTargetsInGazeRange(_gazePoint, gazeRangeCircle, gazeRadius);
        }
        else
        {
            // TryGetLatestGazePoint�� �����ϴ���, Ž���� Ÿ���� �����ǰ� �ִٸ� detectionTime�� ������Ŵ
            if (currentTarget != null)
            {
                detectionTime += Time.deltaTime;
                detectionTime = Mathf.Clamp(detectionTime, 0f, maxDetectionTime);
            }
        }

        // �ֽ� �Ӹ� ��ġ ������ ��������
        if (TobiiGameIntegrationApi.TryGetLatestHeadPose(out _headPose))
        {
            Debug.Log("Head Pose - Position: (" + _headPose.Position.X + ", " + _headPose.Position.Y + ", " + _headPose.Position.Z + ")");
        }

        // Ž���� �ð� �α� ���
        if (currentTarget != null)
        {
            Debug.Log($"Ž�� �ð�: {detectionTime}��");
        }
    }

    void UpdateGazePointVisualization(GazePoint gazePoint)
    {
        // Tobii GazePoint�� X, Y ���� -1 ~ 1 �����̹Ƿ� �̸� 0 ~ Screen.width / Screen.height ������ ��ȯ
        float normalizedX = (gazePoint.X + 1f) / 2f * Screen.width;
        float normalizedY = (gazePoint.Y + 1f) / 2f * Screen.height;

        Vector2 screenPoint = new Vector2(normalizedX, normalizedY);

        // �ʿ信 ���� ���� ����� ������ �� �ֽ��ϴ�.
        float correctionFactorX = 1f; // X�� ���� ���
        float correctionFactorY = 1f; // Y�� ���� ���

        screenPoint.x = Mathf.Clamp(screenPoint.x * correctionFactorX, 0, Screen.width);
        screenPoint.y = Mathf.Clamp(screenPoint.y * correctionFactorY, 0, Screen.height);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gazePointImage.rectTransform.parent as RectTransform,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        gazePointImage.rectTransform.localPosition = localPoint;

        // Gaze ���� ���� ��ġ ������Ʈ (ũ��� ����)
        gazeRangeCircle.rectTransform.localPosition = localPoint;
    }

    void DetectTargetsInGazeRange(GazePoint gazePoint, CircleDrawer circleDrawer, float radius)
    {
        // �ü� �߽��� 2D ��ǥ
        Vector2 gazePosition = new Vector2(
            (gazePoint.X + 1f) / 2f * Screen.width,
            (gazePoint.Y + 1f) / 2f * Screen.height
        );

        // ���� ����� Ÿ�� ã��
        Transform closestTarget = GetClosestTarget(gazePosition, radius);
        bool isSameTarget = (closestTarget == currentTarget);
        bool isSameLastTarget = (closestTarget == lastTarget && Time.time - lastTargetLostTime <= targetGracePeriod);

        if (closestTarget != null)
        {
            if (!isSameTarget && !isSameLastTarget)
            {
                // ���ο� Ÿ���� ã�� ���
                currentTarget = closestTarget;
                detectionTime = 0f; // ���ο� Ÿ���� Ž���Ǹ� Ž�� �ð� �ʱ�ȭ
            }
            else if (isSameLastTarget)
            {
                // ���� �ð� ���� ������ Ÿ���� �ٽ� ã�� ���
                currentTarget = lastTarget;
                detectionTime += Time.deltaTime; // Ž�� �ð� ����
            }
            else
            {
                // ������ Ÿ���� ��� Ž�� ���� ���
                detectionTime += Time.deltaTime;
            }

            detectionTime = Mathf.Clamp(detectionTime, 0f, maxDetectionTime); // �ִ� Ž�� �ð��� 5�ʷ� ����
            Debug.Log($"Ž���� Ÿ��: {closestTarget.name}, Ž�� �ð�: {detectionTime}");
        }
        else
        {
            if (currentTarget != null)
            {
                // Ÿ���� ��ģ �ð��� ���
                lastTargetLostTime = Time.time;
                lastTarget = currentTarget;
            }
            currentTarget = null;
            Debug.Log("Ž���� Ÿ�� ����");
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
        // �ּ� Ž�� �ð����� ª���� 0�� ��ȯ
        if (detectionTime < minDetectionTime)
        {
            return 0f;
        }
        return detectionTime; // Ž�� �ð��� ��ȯ
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
}
