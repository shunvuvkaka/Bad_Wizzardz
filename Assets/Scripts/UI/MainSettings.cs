using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainSettings : MonoBehaviour
{
    public TMP_Text volumeField;
    public Slider volumeSlider;
    public Toggle windowMode;

    public void AdjustWindow()
    {
        if (windowMode.isOn)
        {
            Screen.SetResolution(1920, 1080, true);
        }
        else
        {
            Screen.SetResolution(1920, 1080, false);
        }

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