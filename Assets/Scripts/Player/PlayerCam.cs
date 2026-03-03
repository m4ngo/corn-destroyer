using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    public Transform cam;
    public static float targetFOV = 90;
    public float FOVSmooth = 6.5f;

    public float sens;
    public Transform orientation;
    float xRotation;
    float yRotation;
    Vector2 xy;

    private void Start()
    {
        GUIManager.Instance.HideCursor(true);
    }

    public void OnLook(InputValue value)
    {
        xy = value.Get<Vector2>();
    }

    private void Update()
    {
        float mouseX = xy.x * sens;
        float mouseY = xy.y * sens;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        cam.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        FOV();
    }
    
    void FOV()
    {
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.deltaTime * FOVSmooth);
    }
}
