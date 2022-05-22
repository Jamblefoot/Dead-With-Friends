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

    Transform tran;
    Rigidbody rigid;

    public LayerMask groundLayers;

    float vertical, horizontal;
    // Start is called before the first frame update
    void Start()
    {
        followCam = FindObjectOfType<FollowCam>();
        tran = transform;
        rigid = GetComponent<Rigidbody>();
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
                Vector3 rot = Vector3.RotateTowards(tran.forward, forward * Mathf.Sign(vertCalced), Time.deltaTime * rotSpeed, 0);
                float rotChange = Vector3.SignedAngle(tran.forward, rot, tran.up);
                if(Mathf.Abs(rotChange) <= 0.1f) rotSpeed = 0f;
                tran.LookAt(tran.position + rot, tran.up);
                followCam.ChangeXRotation(rotChange);
            }
            else if(Mathf.Abs(horizontal) > 0.01f)
            {
                rotSpeed = Mathf.Lerp(rotSpeed, rotSpeedMult, Time.deltaTime * acceleration * 0.5f);
                Vector3 rot = Vector3.RotateTowards(tran.forward, followCam.tran.right * Mathf.Sign(horCalced), Time.deltaTime * rotSpeed, 0);
                float rotChange = Vector3.SignedAngle(tran.forward, rot, tran.up);
                if (Mathf.Abs(rotChange) <= 0.1f) rotSpeed = 0f;
                tran.LookAt(tran.position + rot, tran.up);
                followCam.ChangeXRotation(rotChange);
            }
            else
            {
                rotSpeed = 0f;
            }
        }
        else
        {
            move = tran.forward * vertCalced + tran.right * horCalced;
        }

        if(rigid == null)
            tran.position += move;
        else
        {
            rigid.AddForce(move, ForceMode.VelocityChange);
            RaycastHit hit;
            if(Physics.Raycast(tran.position, Vector3.down, out hit, 2.5f, groundLayers, QueryTriggerInteraction.Ignore))
                rigid.AddForce(-Physics.gravity * (4f - hit.distance), ForceMode.Acceleration);
            else rigid.AddForce(-Physics.gravity * 0.5f, ForceMode.Acceleration);
        }
    }
}
