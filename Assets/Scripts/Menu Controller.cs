using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    public string sceneName;
    public string CreditName;
    public void CloseGame() 
    {
        Application.Quit();
    }

    public void LoadScene() 
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void CreditScene()
    {
        SceneManager.LoadScene(CreditName, LoadSceneMode.Single);
    }
}
