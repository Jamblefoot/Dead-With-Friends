using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostDrive : MonoBehaviour
{
    FollowCam followCam;

    public float speedMult = 0.1f;
    public float rotSpeedMult = 5f;
    float acceleration = 10f;
    float vertCalced = 0f;
    float horCalced = 0f;
    float rotSpeed = 0f;

    float vertical, horizontal;
    // Start is called before the first frame update
    void Start()
    {
        followCam = FindObjectOfType<FollowCam>();
    }

    // Update is called once per frame
    void Update()
    {
        vertical = Input.GetAxis("Vertical") * speedMult;
        vertCalced = Mathf.Lerp(vertCalced, vertical, Time.deltaTime * acceleration);
        horizontal = Input.GetAxis("Horizontal") * speedMult;
        horCalced = Mathf.Lerp(horCalced, horizontal, Time.deltaTime * acceleration);
    }

    void FixedUpdate()
    {

        Vector3 move = Vector3.zero;

        if(followCam != null)
        {
            //move followcam forward
            Vector3 forward = Vector3.ProjectOnPlane(followCam.tran.forward, Vector3.up).normalized;
            move = forward * vertCalced + followCam.tran.right * horCalced;
            if(Mathf.Abs(vertical) > 0.01f)
            {
                rotSpeed = Mathf.Lerp(rotSpeed, rotSpeedMult, Time.deltaTime * acceleration * 0.5f);
                Vector3 rot = Vector3.RotateTowards(transform.forward, forward * Mathf.Sign(vertCalced), Time.deltaTime * rotSpeed, 0);
                float rotChange = Vector3.SignedAngle(transform.forward, rot, transform.up);
                transform.LookAt(transform.position + rot, transform.up);
                followCam.ChangeXRotation(rotChange);
            }
            else if(Mathf.Abs(horizontal) > 0.01f)
            {
                rotSpeed = Mathf.Lerp(rotSpeed, rotSpeedMult, Time.deltaTime * acceleration * 0.5f);
                Vector3 rot = Vector3.RotateTowards(transform.forward, followCam.tran.right * Mathf.Sign(horCalced), Time.deltaTime * rotSpeed, 0);
                float rotChange = Vector3.SignedAngle(transform.forward, rot, transform.up);
                transform.LookAt(transform.position + rot, transform.up);
                followCam.ChangeXRotation(rotChange);
            }
            else
            {
                rotSpeed = 0f;
            }
        }
        else
        {
            move = transform.forward * vertCalced + transform.right * horCalced;
        }

        transform.position += move;
    }
}
