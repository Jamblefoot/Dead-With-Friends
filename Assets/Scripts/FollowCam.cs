using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    float xOffset = 0f;
    float yOffset = 0f;

    public Transform tran;
    Transform cameraTran;

    Vector3 lastPos;
    Vector3 cameraTargetLocalPos;

    // Start is called before the first frame update
    void Start()
    {
        tran = transform;
        cameraTran = GetComponentInChildren<Camera>().transform;
        cameraTargetLocalPos = cameraTran.localPosition;

        lastPos = tran.position;
    }

    // Update is called once per frame
    void Update()
    {
        xOffset += Input.GetAxis("Mouse X");
        yOffset -= Input.GetAxis("Mouse Y");
        yOffset = Mathf.Clamp(yOffset, -80f, 80f);

        tran.localRotation = Quaternion.Euler(yOffset, xOffset, 0f);
    }

    void FixedUpdate()
    {
        Vector3 moveDelta = tran.position - lastPos;
        lastPos = tran.position;
        cameraTran.position -= moveDelta;
        cameraTran.localPosition = Vector3.Lerp(cameraTran.localPosition, cameraTargetLocalPos, Time.deltaTime * 10);
    }

    public void ChangeXRotation(float change)
    {
        xOffset -= change;
    }
}
