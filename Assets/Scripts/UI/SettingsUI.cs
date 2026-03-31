using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public TMP_Text volumeField;
    public Slider volumeSlider;
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
    public void AdjustVolume()
    {
        volumeField.text = volumeSlider.value.ToString();

        AudioListener.volume = volumeSlider.value / 100;
    }
    public void Resume()
    {
        GameUI.Instance.currentState = GameUI.UIState.Paused;
        PlayerMovement.Instance.TogglePause();
        gameObject.SetActive(false);
    }
}
