using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMover : MonoBehaviour
{
    // Start is called before the first frame update
    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;
    public float speed = 0.1f;
    public static Vector3 direction;

    // Update is called once per frame
    void Update()
    {
        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }

    void LateUpdate()
    {
        float hor = CrossPlatformInputManager.GetAxis("Horizontal");
        float ver = CrossPlatformInputManager.GetAxis("Vertical");
        float up = Input.GetKey(KeyCode.LeftShift) ? 0.5f : 0f;
        float down = Input.GetKey(KeyCode.Space) ? 0.5f : 0f;
        var input = new Vector3(hor, ver, up - down) * speed;
        Vector3 vector3 = this.transform.forward * input.y + this.transform.right * input.x + Vector3.up * input.z;
        this.transform.position += vector3;
        if (vector3 != Vector3.zero)
        {
            direction = this.transform.forward * input.y + this.transform.right * input.x + Vector3.up * input.z;
        }
    }
}
