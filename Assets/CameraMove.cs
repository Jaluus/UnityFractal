using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        float moveY = Input.GetAxis("Horizontal");
        float moveX = Input.GetAxis("Vertical");
        float moveZ = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            moveZ += 1;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveZ -= 1;
        }

        transform.position = transform.position + Vector3.forward * moveX + Vector3.right * moveY + Vector3.up * moveZ;

    }
}
