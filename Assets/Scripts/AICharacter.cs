using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICharacter : MonoBehaviour
{
    Animator anim;
    NavMeshAgent agent;
    Transform tran;

    public Transform target;

    public bool scared = false;

    public GameObject ghostPrefab;

    public bool alive = true;

    public Seat currentSeat;
    public LayerMask seatLayers;

    Transform mainParent;

    public bool possessed;
    [Tooltip("this is the camera anchor point. If head moves, neck might be better")]
    public Transform head;

    public bool dontSpawnGhost = false;

    Vector3 lastForward;

    float wanderTime;
    Vector3 wanderPos;

    bool followRigidbody = false;
    Rigidbody rigid;

    public float walkSpeed = 2f;
    public float runSpeed = 10f;

    

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        tran = transform;

        rigid = GetComponentInChildren<Rigidbody>();

        SkinnedMeshRenderer rend = GetComponentInChildren<SkinnedMeshRenderer>();
        List<Material> mats = new List<Material>();
        rend.GetMaterials(mats);
        for(int i = 0; i < mats.Count; i++)
        {
            Color col = new Color(Random.value, Random.value, Random.value, 1);
            mats[i].color = col;
        }

        foreach(Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }

        mainParent = tran.parent;
        if(currentSeat != null)
            EnterSeat(currentSeat);

        lastForward = tran.forward;

        wanderPos = tran.position + new Vector3(Random.Range(-50f, 50f), 0f, Random.Range(-50f, 50f));
    }

    void Seek(Vector3 location)
    {
        if(!agent.enabled) return;

        agent.SetDestination(location);
    }
    void Flee(Vector3 location)
    {
        if(!agent.enabled) return;

        Vector3 fleeVector = location - tran.position;
        agent.SetDestination(tran.position - fleeVector);
    }

    public void Kill()
    {
        if(!alive) return;

        alive = false;
        StartCoroutine(CoKill());
    }

    IEnumerator CoKill()
    {
        yield return new WaitForFixedUpdate();
        if (currentSeat != null)
            LeaveSeat();
        agent.enabled = false;
        //transform.position = transform.position + Vector3.up * 2;
        anim.enabled = false;

        if(!dontSpawnGhost)
            Instantiate(ghostPrefab, transform.position, transform.rotation);

        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
        }
    }

    public void Fall()
    {
        agent.enabled = false;
        anim.enabled = false;
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
        }

        followRigidbody = true;
    }

    public void EnterSeat(Seat seat)
    {
        if(seat.occupant != null && seat.occupant != this)
            return;
        
        seat.occupant = this;
        agent.enabled = false;
        anim.SetBool("walking", false);
        anim.SetBool("running", false);
        anim.SetBool("sitting", true);
        Collider col = seat.GetComponentInParent<Collider>();
        foreach(Collider c in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(col, c, true);
        }
        tran.parent = seat.transform;
        tran.localPosition = Vector3.zero;
        tran.localRotation = Quaternion.identity;
        currentSeat = seat;
    }

    public void LeaveSeat()
    {
        anim.SetBool("sitting", false);
        anim.SetBool("walking", false);
        anim.SetBool("running", false);
        tran.parent = mainParent;
        agent.enabled = true;

        if(currentSeat != null && currentSeat.occupant == this)
            currentSeat.occupant = null;

        /*if (currentSeat != null)
        {
            Collider col = currentSeat.GetComponentInParent<Collider>();
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(col, c, false);
            }
        }*/

        currentSeat = null;
    }

    bool CheckForSeat()
    {
        Debug.Log("Looking for seat");
        RaycastHit hit;
        if(Physics.Raycast(head.position, GameControl.instance.followCam.tran.forward, out hit, 2f, seatLayers, QueryTriggerInteraction.Collide))
        {
            Seat s = hit.transform.GetComponentInChildren<Seat>();
            if(s != null && s.occupant == null)
            {
                EnterSeat(s);
                return true;
            }
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameControl.instance.inMenu) 
        {
            agent.speed = 0;
            return;
        }

        if(!alive) return;

        if (head.position.y < GameControl.instance.waterLevel)
        {
            //if(currentSeat != null)
            //    LeaveSeat();
            Kill();
            return;
        }

        if(currentSeat != null) 
        {
            if(possessed && Input.GetButtonDown("Fire2"))
                LeaveSeat();

            return;
        }

        if (possessed)
        {
            if(Input.GetButtonDown("Fire1"))
            {
                if(CheckForSeat()) return;
            }

            if(currentSeat != null) return;

            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");
            bool sprint = Input.GetButton("Fire3");

            Seek(tran.position + Vector3.ProjectOnPlane(GameControl.instance.followCam.tran.forward * vertical * 5 + GameControl.instance.followCam.tran.right * horizontal * 5, Vector3.up));

            if(sprint) 
            {
                anim.SetBool("running", true);
                agent.speed = runSpeed;
            }
            else 
            {
                anim.SetBool("running", false);
                agent.speed = walkSpeed;
            }
            if ((agent.destination - tran.position).magnitude > agent.stoppingDistance)
            {
                anim.SetBool("walking", true);
            }
            else anim.SetBool("walking", false);

            return;
        }

        if(!scared)
        {
            if(target != null)
                Seek(target.position);
            else 
            {
                Seek(wanderPos);
                wanderTime -= Time.deltaTime;
                if(wanderTime < 0f)
                {
                    wanderTime = Random.Range(10f, 30f);
                    wanderPos = tran.position + new Vector3(Random.Range(-50f, 50f), 0f, Random.Range(-50f, 50f));
                }
            }

            anim.SetBool("running", false);
            if((agent.destination - tran.position).magnitude > agent.stoppingDistance)
            {
                anim.SetBool("walking", true);
                agent.speed = walkSpeed;
            }
            else anim.SetBool("walking", false);
        }
        else
        {
            if (target != null)
                Flee(target.position);

            if ((agent.destination - tran.position).magnitude > agent.stoppingDistance)
            {
                anim.SetBool("running", true);
                agent.speed = runSpeed;
            }
            else anim.SetBool("running", false);
        }

        
    }

    void LateUpdate()
    {
        if(tran.forward != lastForward)
        {
            if(possessed)
            {
                float rotChange = Vector3.SignedAngle(lastForward, tran.forward, Vector3.up);
                GameControl.instance.followCam.ChangeXRotation(rotChange);
            }

            lastForward = tran.forward;
        }

        if(followRigidbody && alive)
        {
            List<Transform> children = new List<Transform>();
            foreach(Transform child in tran)
            {
                children.Add(child);
                child.parent = null;
            }
            transform.position = rigid.position;
            foreach(Transform child in children)
            {
                child.parent = tran;
            }
        }
    }
}
