using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    public GameObject player {get; private set;}
    public GameObject barrier;
    public TMP_Text scoreText;
    public int score = 0;
    public int addScore;
    public int totalScore;
    public Road road {get; private set;}
    public static GameplayManager Instance;
    private AsyncOperation asyncLoad;
    private bool done = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        //StartCoroutine(LoadSceneAsync("MainLevel"));
    }
    void Update()
    {
        if (!done)
            return;
        
        barrier.transform.position = road.roadPoints[10];
        barrier.transform.rotation = Quaternion.FromToRotation(Vector3.right, road.pointNormals[10]);

        scoreText.text = totalScore.ToString();

        score = road.globalIndex * 10;
        totalScore = score + addScore;
    }
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (asyncLoad.isDone)
        {
            player = GameObject.FindWithTag("Player");
            barrier = GameObject.FindWithTag("Finish");
            scoreText = GameObject.FindWithTag("Score").GetComponent<TMP_Text>();
            road = Road.Instance;

            while (road.neededPoints < road.roadPoints.Count - 3 || road.pointNormals.Count != road.roadPoints.Count)
            {
                yield return null;
            }

            player.transform.position = road.roadPoints[150];
            player.transform.rotation = Quaternion.FromToRotation(Vector3.left, road.pointNormals[150]);
        }

        done = true;
    }
}