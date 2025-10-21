using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float x;
    private float y;
    private float RotationX;
    private float RotationY;
    public GameObject Camera;

    public float movementSpeed = 1.0f;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        // Movement Input
        if (Input.GetKey(KeyCode.W)) // forwards movement
        {
            transform.transform.Translate(new Vector3(0, 0, movementSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.S)) // backwards movement
        {
            transform.transform.Translate(new Vector3(0, 0, -movementSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.D)) // right strafe
        {
            transform.transform.Translate(new Vector3(movementSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.A)) // left strafe
        {
            transform.transform.Translate(new Vector3(-movementSpeed * Time.deltaTime, 0, 0));
        }

        // Mouse Input
        x = Input.GetAxis("Mouse X");
        y = Input.GetAxis("Mouse Y");

        RotationX += x;
        RotationY -= y;

        RotationY = Mathf.Clamp(RotationY, -65f, 75f);

        transform.localEulerAngles = new Vector3(0, RotationX, 0);
        Camera.transform.localEulerAngles = new Vector3(RotationY, 0, 0);
    }
}
