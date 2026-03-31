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
        //TODO
    }
}
