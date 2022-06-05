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
    public Rigidbody rigid;
    SkinnedMeshRenderer rend;

    public LayerMask groundLayers;
    public LayerMask characterLayer;
    public LayerMask barrierLayer;

    float vertical, horizontal;
    bool possessing;
    public AICharacter possessed;
    Material normalMaterial;
    public Material possessMaterial;

    [SerializeField] bool randomStart = true;
    public bool stillAlive = false;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip inhaleClip;
    [SerializeField] AudioClip exhaleClip;

    [SerializeField] GameObject cloudPrefab;
    bool isCloud = false;

    // Start is called before the first frame update
    void Start()
    {
        followCam = FindObjectOfType<FollowCam>();
        tran = transform;
        rigid = GetComponent<Rigidbody>();
        rend = GetComponent<SkinnedMeshRenderer>();
        normalMaterial = rend.material;

        if(randomStart)
            possessed = GameControl.instance.GetRandomPerson();

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
        if(stillAlive || isCloud) return;

        vertical = Input.GetAxis("Vertical") * speedMult;
        vertCalced = Mathf.Lerp(vertCalced, vertical, Time.deltaTime * acceleration);
        horizontal = Input.GetAxis("Horizontal") * speedMult;
        horCalced = Mathf.Lerp(horCalced, horizontal, Time.deltaTime * acceleration);

        if(Input.GetButtonDown("Jump") && !GameControl.instance.inMenu)
        {
            if(!possessing)
                Possess();
            else StopPossess();
        }

        if(GameControl.instance.livingCount <= 0 && tran.position.y > 150f)
        {
            BecomeCloud();
        }
    }

    void FixedUpdate()
    {
        if(isCloud) return;

        if(possessing) 
        {
            if(!possessed.alive)
            {
                StopPossess();
                return;
            }
            if(rigid != null)
                rigid.interpolation = RigidbodyInterpolation.None;
            tran.localPosition = Vector3.up * 0.5f;//Vector3.zero;
            tran.rotation = tran.parent.rotation;
            return;
        }
        else
        {
            if(rigid != null)
                rigid.interpolation = RigidbodyInterpolation.Interpolate;
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
        else //followCam = null
        {
            move = tran.forward * vertCalced + tran.right * horCalced;
        }

        
        if (tran.position.y < GameControl.instance.waterLevel)
            move = followCam.tran.forward * vertCalced + followCam.tran.right * horCalced;
            //move = move * 0.25f + followCam.tran.forward * Mathf.Abs(Vector3.Dot(followCam.tran.forward, Vector3.up)) * vertCalced;

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
        if(Physics.SphereCast(tran.position, 1f, followCam.tran.forward, out hit, 3f, characterLayer, QueryTriggerInteraction.Ignore))
        {
            AICharacter aic = hit.transform.GetComponentInParent<AICharacter>();
            if(!aic.alive) return false;

            possessed = aic;
            possessed.possessed = true;
            possessing = true;
            rend.material = possessMaterial;
            rigid.isKinematic = true;
            tran.parent = possessed.head;
            tran.localPosition = Vector3.up * 0.5f;//Vector3.zero;
            tran.rotation = tran.parent.rotation;

            audioSource.pitch = Random.Range(0.8f, 1.5f);
            audioSource.PlayOneShot(inhaleClip);
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
        tran.localPosition = Vector3.up * 0.5f;//Vector3.zero;
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
        tran.LookAt(tran.position + Vector3.ProjectOnPlane(forward, Vector3.up).normalized, Vector3.up);

        audioSource.pitch = 1;
        audioSource.PlayOneShot(exhaleClip);

        MoveAboveGround();

        if(GameControl.instance.autoSlomo)
            GameControl.instance.SetSlomo(false);

    }

    void MoveAboveGround()
    {
        RaycastHit hit;
        Vector3 barrierPos = new Vector3(tran.position.x, 0, tran.position.z);
        if(Physics.Raycast(Vector3.down * 100, barrierPos.normalized, out hit, barrierPos.magnitude - 1f, barrierLayer, QueryTriggerInteraction.Ignore))
        {
            tran.position = new Vector3(hit.point.x, tran.position.y, hit.point.z);
        }

        if(!Physics.Raycast(tran.position, Vector3.down, Mathf.Infinity, groundLayers, QueryTriggerInteraction.Ignore))
        {
            Physics.Raycast(tran.position + Vector3.up * 1000, Vector3.down, out hit, 1000, groundLayers, QueryTriggerInteraction.Ignore);
            tran.position = hit.point + Vector3.up;
        }
    }

    void BecomeCloud()
    {
        isCloud = true;
        rend.enabled = false;
        rigid.isKinematic = true;
        followCam.SetDistance(50);
        Instantiate(cloudPrefab, tran.position, tran.rotation);

        foreach(GhostAI g in FindObjectsOfType<GhostAI>())
        {
            g.gameObject.SetActive(false);
        }
    }
}
