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

    public LayerMask groundLayers;

    float followDistance;

    // Start is called before the first frame update
    void Start()
    {
        tran = transform;
        cameraTran = GetComponentInChildren<Camera>().transform;
        cameraTargetLocalPos = cameraTran.localPosition;

        followDistance = (tran.position - cameraTran.position).magnitude;

        lastPos = tran.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameControl.instance.inMenu) return;

        xOffset += Input.GetAxis("Mouse X");
        yOffset -= Input.GetAxis("Mouse Y");
        yOffset = Mathf.Clamp(yOffset, -80f, 80f);

        //tran.localRotation = Quaternion.Euler(yOffset, xOffset, 0f);

        Vector3 camVector = (cameraTran.position - tran.position).normalized;
        RaycastHit hit;
        if(Physics.Raycast(tran.position, camVector, out hit, followDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            cameraTran.position = tran.position + camVector * (hit.distance - 1f);
        }
    }

    void FixedUpdate()
    {
        if(GameControl.instance.inMenu) return;

        Vector3 moveDelta = tran.position - lastPos;
        lastPos = tran.position;
        cameraTran.position -= moveDelta;
        cameraTran.localPosition = Vector3.Lerp(cameraTran.localPosition, cameraTargetLocalPos, Time.deltaTime * 10);
    }

    void LateUpdate()
    {
        tran.localRotation = Quaternion.Euler(yOffset, xOffset, 0f);
    }

    public void ChangeXRotation(float change)
    {
        xOffset -= change;
    }

    public void ChangeDistance(float change)
    {
        cameraTargetLocalPos = cameraTargetLocalPos * (change / followDistance);
        followDistance = change;
    }
}
