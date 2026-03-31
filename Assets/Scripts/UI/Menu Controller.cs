using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    public string sceneName;
    public GameObject main;
    public GameObject play;
    public GameObject credits;
    public Animator panel;
    public Animator bg;
    public void CloseGame() 
    {
        Application.Quit();
    }

    public void Credits()
    {
        credits.SetActive(true);
    }
    public void Play()
    {
        panel.SetTrigger("Play");
        bg.SetTrigger("Play");
        main.SetActive(false);
        play.SetActive(true);
        credits.SetActive(false);
    }

    public void Back()
    {
        Debug.Log("back");
        panel.SetTrigger("Main");
        bg.SetTrigger("Main");
        main.SetActive(true);
        play.SetActive(false);
        credits.SetActive(false);
    }


    public void LoadScene() 
    {
        GameplayManager.Instance.LoadScene("MainLevel");
    }
}
