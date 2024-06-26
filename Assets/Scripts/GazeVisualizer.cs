using System;
using System.Runtime.InteropServices;
using Tobii.GameIntegration.Net;
using UnityEngine;
using UnityEngine.UI;

public class GazeVisualizer : MonoBehaviour
{
    // UI Element to represent the gaze point
    public RectTransform gazePointer;

    // Update rate for checking gaze data
    public float updateRate = 0.1f;
    private float nextUpdateTime = 0f;

    private bool _isApiInitialized = false;

    void Start()
    {
        try
        {
            // Initialize Tobii API
            Debug.Log("Initializing Tobii API");
            TobiiGameIntegrationApi.SetApplicationName("UnityGazeVisualizer");

            // Track the specific tracker
            if (TobiiGameIntegrationApi.TrackTracker("tobii-prp://IS5FF-100203350232"))
            {
                Debug.Log("Tracker registered successfully.");
            }
            else
            {
                Debug.LogWarning("Failed to register tracker.");
            }

            // Additional tracker info for debugging
            var trackerInfo = TobiiGameIntegrationApi.GetTrackerInfo();
            if (trackerInfo != null)
            {
                Debug.Log("Tracker Info: " + trackerInfo.FriendlyName + ", " + trackerInfo.Url);
                Debug.Log("Is Tracker Connected: " + TobiiGameIntegrationApi.IsTrackerConnected());
                Debug.Log("Is Tracker Enabled: " + TobiiGameIntegrationApi.IsTrackerEnabled());
            }
            else
            {
                Debug.LogWarning("Failed to get tracker info.");
            }

            _isApiInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception during API initialization: " + ex.Message);
            Debug.LogError("Stack Trace: " + ex.StackTrace);
        }
    }

    void Update()
    {
        // Call the Tobii API Update method to keep the state up-to-date
        TobiiGameIntegrationApi.Update();

        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateRate;
            UpdateGazePoint();
        }
    }

    void UpdateGazePoint()
    {
        GazePoint gazePoint;
        bool success = TobiiGameIntegrationApi.TryGetLatestGazePoint(out gazePoint);

        //Debug.Log("TryGetLatestGazePoint success: " + success);

        if (success)
        {
            Debug.Log("Gaze Point - X: " + gazePoint.X + ", Y: " + gazePoint.Y);

            Vector2 screenPoint = new Vector2(gazePoint.X * Screen.width, gazePoint.Y * Screen.height);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gazePointer.parent as RectTransform,
                screenPoint,
                null,
                out Vector2 localPoint
            );

            gazePointer.localPosition = localPoint;
        }
        else
        {
            Debug.LogWarning("Failed to get the latest gaze point.");
        }
    }

    void OnApplicationQuit()
    {
        if (_isApiInitialized)
        {
            Debug.Log("Shutting down Tobii API");
            TobiiGameIntegrationApi.Shutdown();
        }
    }
}
