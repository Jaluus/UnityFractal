using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMove : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float mouseSens = 1f;

    public GameObject XField;
    public GameObject YField;
    public GameObject ZField;

    public GameObject XRotField;
    public GameObject YRotField;
    public GameObject ZRotField;

    float xRot = 0;
    float yRot = 0;
    float zRot = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!OptionsMenu.isActive)
        {
            MoveCamera();
            PanCamera();
        }
    }

    private void PanCamera()
    {
        float PanX = Input.GetAxis("Mouse X") * mouseSens;
        float PanY = Input.GetAxis("Mouse Y") * mouseSens;
        float PanZ = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            PanZ += mouseSens;
        }
        if (Input.GetKey(KeyCode.E))
        {
            PanZ -= mouseSens;
        }

        xRot += PanX;
        yRot += PanY;
        zRot += PanZ;

        transform.Rotate((Vector3.up * PanX + Vector3.right * PanY + Vector3.forward * PanZ)* Time.deltaTime);
    }

    private void MoveCamera()
    {
        float moveY = Input.GetAxis("Horizontal") * moveSpeed;
        float moveX = Input.GetAxis("Vertical") * moveSpeed;
        float moveZ = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            moveZ += moveSpeed;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveZ -= moveSpeed;
        }
        transform.localPosition += (transform.forward * moveX + transform.right * moveY + transform.up * moveZ) * Time.deltaTime;
    }

    public void SetPosition()
    {
        int posX = Int16.Parse(XField.GetComponent<InputField>().text);
        int posY = Int16.Parse(YField.GetComponent<InputField>().text);
        int posZ = Int16.Parse(ZField.GetComponent<InputField>().text);

        transform.position = new Vector3(posX, posY, posZ);
    }
    public void SetRotation()
    {
        int rotX = Int16.Parse(XRotField.GetComponent<InputField>().text);
        int rotY = Int16.Parse(YRotField.GetComponent<InputField>().text);
        int rotZ = Int16.Parse(ZRotField.GetComponent<InputField>().text);

        transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
    }
}
