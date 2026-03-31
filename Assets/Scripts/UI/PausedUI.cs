using UnityEngine;

public class PausedUI : MonoBehaviour
{
    public void Resume()
    {
        PlayerMovement.Instance.TogglePause();
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void Settings()
    {
        gameObject.SetActive(false);
        GameUI.Instance.currentState = GameUI.UIState.Settings;
        GameUI.Instance.settings.SetActive(true);
    }
}
