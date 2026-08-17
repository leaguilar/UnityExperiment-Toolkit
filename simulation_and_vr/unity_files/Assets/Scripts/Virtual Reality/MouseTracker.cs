using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* EBD FS20, Raphaël Baur
 * In case of bugs or suggestions, feel free to contact rabaur@student.ethz.ch.
 */
public class MouseTracker : MonoBehaviour
{
    [Tooltip("How fast is your player reacting to mouse-movement.")]
    public float mouseSensitivity = 100.0f;
    [Tooltip("The body of the character.")]
    public Transform playerBody;
    [Tooltip("How far back can the character tilt its head back (degrees).")]
    public float maxDorsal = 60.0f;
    [Tooltip("How far forward can the character tilt its head forward (degrees).")]
    public float maxVentral = 60.0f;
    public bool isLocalPlayer = true;
    private float xRotation = 0.0f;

    void Start()
    {
        if (enabled && isLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90.0f, 90.0f);

        transform.localRotation = Quaternion.Euler(xRotation, 0.0f, 0.0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
