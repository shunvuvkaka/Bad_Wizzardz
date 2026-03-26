using UnityEngine;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    public TMP_Text volumeField;
    public TMP_Dropdown windowMode;

    public void AdjustWindow()
    {
        FullScreenMode mode;

        switch (windowMode.value)
        {
            case 0:
                mode = FullScreenMode.Windowed;
                break;
            case 1:
                mode = FullScreenMode.FullScreenWindow;
                break;
            default:
                mode = FullScreenMode.Windowed;
                break;
        }

        Screen.SetResolution(1920, 1080, mode);
    }
}
