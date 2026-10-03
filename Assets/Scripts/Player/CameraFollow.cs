using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    public float mouseSensitivity = 150f;
    public float distance = 5f;
    public float height = 1.8f;
    public float shoulderOffset = 1.2f;

    private float yaw = 0f;
    private float pitch = 10f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float mouseX = Input.GetAxis("Mouse X") *
                       mouseSensitivity * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") *
                       mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, -20f, 60f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 target =
            player.position + Vector3.up * height;

        Vector3 cameraPosition =
            target
            - rotation * Vector3.forward * distance
            + rotation * Vector3.right * shoulderOffset;

        transform.position = cameraPosition;

        transform.LookAt(target);
    }
}
