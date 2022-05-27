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

    public Transform tran;
    Rigidbody rigid;
    SkinnedMeshRenderer rend;

    public LayerMask groundLayers;
    public LayerMask characterLayer;

    float vertical, horizontal;
    bool possessing;
    public AICharacter possessed;
    Material normalMaterial;
    public Material possessMaterial;

    public bool stillAlive = false;
    // Start is called before the first frame update
    void Start()
    {
        followCam = FindObjectOfType<FollowCam>();
        tran = transform;
        rigid = GetComponent<Rigidbody>();
        rend = GetComponent<SkinnedMeshRenderer>();
        normalMaterial = rend.material;

        if(possessed != null)
        {
            rend.enabled = false;
            possessed.dontSpawnGhost = true;
            stillAlive = true;
            Possess(possessed);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(stillAlive) return;

        vertical = Input.GetAxis("Vertical") * speedMult;
        vertCalced = Mathf.Lerp(vertCalced, vertical, Time.deltaTime * acceleration);
        horizontal = Input.GetAxis("Horizontal") * speedMult;
        horCalced = Mathf.Lerp(horCalced, horizontal, Time.deltaTime * acceleration);

        if(Input.GetButtonDown("Jump"))
        {
            if(!possessing)
                Possess();
            else StopPossess();
        }
    }

    void FixedUpdate()
    {
        if(possessing) 
        {
            if(!possessed.alive)
            {
                StopPossess();
                return;
            }
            tran.localPosition = Vector3.zero;
            tran.rotation = tran.parent.rotation;
            return;
        }

        if(GameControl.instance.inMenu) 
        {
            if(rigid != null)
                rigid.isKinematic = true;
            return;
        }
        else
        {
            if(rigid != null && rigid.isKinematic)
                rigid.isKinematic = false;
        }

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

    bool Possess()
    {
        RaycastHit hit;
        if(Physics.Raycast(tran.position, tran.forward, out hit, 2f, characterLayer, QueryTriggerInteraction.Ignore))
        {
            AICharacter aic = hit.transform.GetComponentInParent<AICharacter>();
            if(!aic.alive) return false;

            possessed = aic;
            possessed.possessed = true;
            possessing = true;
            rend.material = possessMaterial;
            rigid.isKinematic = true;
            tran.parent = possessed.head;
            tran.localPosition = Vector3.zero;
            tran.rotation = tran.parent.rotation;
            return true;
        }

        return false;
    }
    bool Possess(AICharacter person)
    {
        person.possessed = true;
        possessing = true;
        rend.material = possessMaterial;
        rigid.isKinematic = true;
        tran.parent = possessed.head;
        tran.localPosition = Vector3.zero;
        tran.rotation = tran.parent.rotation;
        return true;
    }
    void StopPossess()
    {
        possessing = false;
        possessed.possessed = false;
        possessed = null;
        stillAlive = false;
        rend.enabled = true;
        rend.material = normalMaterial;
        rigid.isKinematic = false;
        tran.parent = null;
        Vector3 forward = tran.forward;
        tran.rotation = Quaternion.identity;
        //tran.LookAt(Vector3.ProjectOnPlane(tran.position + forward, Vector3.up).normalized, Vector3.up);

    }
}
