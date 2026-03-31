using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadUI : MonoBehaviour
{
    public void Restart()
    {
        GameplayManager.Instance.LoadScene("MainLevel");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
