using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    float xOffset = 0f;
    float yOffset = 0f;

    public Transform tran;
    public Transform cameraTran;

    Vector3 lastPos;
    Vector3 lastVel = Vector3.zero;
    Vector3 cameraTargetLocalPos;
    Vector3 cameraTargetLocalPosAdjusted;

    public LayerMask groundLayers;
    public LayerMask terrainLayer;

    float followDistance;

    bool fixedOccurred = false;

    public enum UpdateMode{ Fixed, Late};
    public UpdateMode updateMode = UpdateMode.Late;

    float lastHitDist = 0f;

    //Transform followTarget;

    //bool softenVertical;

    // Start is called before the first frame update
    void Start()
    {
        tran = transform;
        cameraTran = GetComponentInChildren<Camera>().transform;
        cameraTargetLocalPos = cameraTran.localPosition;

        followDistance = (tran.position - cameraTran.position).magnitude;

        lastPos = tran.position;
        //lastLocalPos = tran.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameControl.instance.inMenu && !GameControl.instance.photoMode) return;

        //if(Input.GetKeyDown(KeyCode.V)) softenVertical = !softenVertical;

        xOffset += Input.GetAxis("Mouse X");
        yOffset -= Input.GetAxis("Mouse Y");
        yOffset = Mathf.Clamp(yOffset, -80f, 80f);

        //tran.localRotation = Quaternion.Euler(yOffset, xOffset, 0f);

        /*Vector3 camVector = (cameraTran.position - tran.position).normalized;
        RaycastHit hit;
        if(Physics.Raycast(tran.position, camVector, out hit, followDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            cameraTran.position = tran.position + camVector * (hit.distance - 1f);
        }*/

        if(GameControl.instance.player.rigid.interpolation != RigidbodyInterpolation.None)
            updateMode = UpdateMode.Late;
        else updateMode = UpdateMode.Fixed;
        if(GameControl.instance.player.possessed != null)
        { 
            if(GameControl.instance.player.possessed.currentSeat == null)
            { 
                if(!GameControl.instance.player.possessed.floppy)
                    updateMode = UpdateMode.Late;
            }
            else
            {
                if(tran.parent == GameControl.instance.player.tran)
                {
                    tran.parent = null;
                    //followTarget = GameControl.instance.player.tran;//.possessed.currentSeat.root;
                }
            }
        }

        if(tran.parent == null)
        {
            if(GameControl.instance.player.possessed == null || GameControl.instance.player.possessed.currentSeat == null)
            {
                tran.parent = GameControl.instance.player.tran;
                tran.localPosition = Vector3.zero;
                //tran.localRotation = Quaternion.identity;
            }
        }
    }

    void FixedUpdate()
    {
        fixedOccurred = true;
    }

    void LateUpdate()
    {
        //tran.localRotation = Quaternion.Euler(yOffset, xOffset, 0f);

        if (GameControl.instance.inMenu && !GameControl.instance.photoMode)
        {
            fixedOccurred = false;
            return;
        }

        float deltaTime = Time.deltaTime;
        if(updateMode == UpdateMode.Fixed) deltaTime = Mathf.Max(Time.fixedDeltaTime, Time.deltaTime);

        if((updateMode == UpdateMode.Fixed && fixedOccurred) || updateMode == UpdateMode.Late) 
        {
            tran.localRotation = Quaternion.Euler(yOffset, xOffset, 0f);
            ReposeCamDistance();

            if (tran.parent != null)
            {
            Vector3 moveDelta = tran.position - lastPos;
            Vector3 vertDelta = Vector3.zero;
            AICharacter aic = GameControl.instance.player.possessed;
            if (aic != null)
            {
                if(aic.floppy)
                    moveDelta = aic.GetComponentInChildren<Rigidbody>().velocity * deltaTime;
                /*else if(aic.currentSeat != null && aic.currentSeat.root != null)
                {
                    lastVel = Vector3.Lerp(lastVel, aic.currentSeat.root.GetComponentInChildren<Rigidbody>().velocity, deltaTime);
                    moveDelta = lastVel * deltaTime;
                    vertDelta = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(transform.InverseTransformVector(moveDelta), transform.forward), transform.right);
                }
                else lastVel = Vector3.zero;*/
            }
            //else lastVel = Vector3.zero;
            
            lastPos = tran.position;

            
            cameraTran.position -= moveDelta;
            cameraTran.localPosition = Vector3.Lerp(cameraTran.localPosition - vertDelta, cameraTargetLocalPosAdjusted - vertDelta, deltaTime * 10);
            }
            else //tran.parent == null - is in car
            {
                //Vector3 moveDelta = Vector3.ProjectOnPlane(followTarget.position - lastPos, followTarget.up);
                lastPos = GameControl.instance.player.tran.position;
                tran.position = Vector3.Lerp(tran.position, GameControl.instance.player.tran.position, deltaTime * 10);
                cameraTran.localPosition = cameraTargetLocalPosAdjusted;
            }

            fixedOccurred = false;

            
        }

    }

    void ReposeCamDistance()
    {
        Vector3 camVector = (cameraTran.position - tran.position).normalized;

        LayerMask mask = followDistance < 20f ? groundLayers : terrainLayer;
        //{
        RaycastHit hit;
        if (Physics.Raycast(tran.position, camVector, out hit, followDistance, mask, QueryTriggerInteraction.Ignore))
        {
            //float dist = Mathf.Lerp(lastHitDist, hit.distance, Time.deltaTime);
            //lastHitDist = dist;
            
            cameraTran.position = tran.position + camVector * (hit.distance - 1f);
            cameraTargetLocalPosAdjusted = cameraTargetLocalPos.normalized * (hit.distance - 1f);

            return;
        }
        //}
        
        cameraTargetLocalPosAdjusted = cameraTargetLocalPos.normalized * followDistance;
    }

    public void ChangeXRotation(float change)
    {
        xOffset -= change;
    }

    public void SetDistance(float value)
    {
        //cameraTargetLocalPos = cameraTargetLocalPos * (change / followDistance);
        followDistance = value;//Mathf.Max(value, 1f);
    }
    public void ChangeDistance(float change)
    {
        followDistance += change;
        followDistance = Mathf.Clamp(followDistance, 2f, 100f);
    }
    public float GetDistance()
    {
        return followDistance;
    }
}
