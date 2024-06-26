using UnityEngine;

public class SetResolution : MonoBehaviour
{
    void Start()
    {
        // 현재 모니터의 해상도를 가져옵니다.
        int screenWidth = Display.main.systemWidth;
        int screenHeight = Display.main.systemHeight;

        // 해상도를 모니터의 해상도로 설정합니다.
        Screen.SetResolution(screenWidth, screenHeight, true);
    }
}

