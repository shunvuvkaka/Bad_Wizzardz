using UnityEngine;

public class ObjectUI : MonoBehaviour
{
    public GameObject prevImage;
    public GameObject baseImage;
    public static ObjectUI Instance;
    void Awake()
    {
        Instance = this;
    }
    public void SelectObject(GameObject picture)
    {
        picture.SetActive(true);

        if(prevImage != null && prevImage != picture)
            prevImage.SetActive(false);

        prevImage = picture;
    }
}
