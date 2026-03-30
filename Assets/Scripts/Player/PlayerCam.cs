using UnityEngine;
public class PlayerCamera : MonoBehaviour
{

    private Transform playerBody;
    private float pitch = 0f;
    private readonly float sensitivity = 2f;
    public bool casting = false;
    public static PlayerCamera Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerBody = transform.parent;
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!casting)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;   
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            pitch -= mouseY;                             
            pitch = Mathf.Clamp(pitch, -90f, 90f);       

            // Apply vertical rotation to camera
            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            // Apply horizontal rotation to player (yaw)
            playerBody.Rotate(Vector3.up * mouseX);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}