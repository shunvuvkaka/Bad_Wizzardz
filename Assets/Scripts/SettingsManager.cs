using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Application.targetFrameRate = Mathf.RoundToInt(120);
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
        AdjustCamera();
    }

    void AdjustCamera() 
    {
        float targetRatio = 16.0f / 9.0f; // Your target
        float windowRatio = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowRatio / targetRatio;

        Camera camera = Camera.main;

        if (scaleHeight < 1.0f) 
        {
            // Letterbox (Bars top/bottom)
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            camera.rect = rect;
        } 
        else 
        {
            // Pillarbox (Bars sides)
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
    }
}
}
