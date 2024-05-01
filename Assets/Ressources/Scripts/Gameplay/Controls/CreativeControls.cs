using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeControls : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Camera cam;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float mouseSensitivity = 100f;

    private void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);
        movement = this.transform.TransformDirection(movement);

        if (Input.GetKey(KeyCode.LeftControl))
        {
            movement *= 2;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            movement.y += 1;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movement.y -= 1;
        }

        controller.Move(movement * movementSpeed * Time.deltaTime);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
        cam.transform.Rotate(Vector3.left * mouseY);
    }
}
