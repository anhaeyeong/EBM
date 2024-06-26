using UnityEngine;

public class SetResolution : MonoBehaviour
{
    void Start()
    {
        // ���� ������� �ػ󵵸� �����ɴϴ�.
        int screenWidth = Display.main.systemWidth;
        int screenHeight = Display.main.systemHeight;

        // �ػ󵵸� ������� �ػ󵵷� �����մϴ�.
        Screen.SetResolution(screenWidth, screenHeight, true);
    }
}

