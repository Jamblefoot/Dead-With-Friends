using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICharacter : MonoBehaviour
{
    Animator anim;
    public NavMeshAgent agent;
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

    Vector3 fleeVector = Vector3.zero;
    float fleeTimer;
    bool justSawGhost = false;

    public bool onFire = false;
    public float fireTimer;
    float fireWanderTimer;

    bool followRigidbody = false;
    //Rigidbody rigid;
    Rigidbody[] rigids;

    public float walkSpeed = 2f;
    public float runSpeed = 10f;

    [SerializeField] AudioSource footstepAudio;
    [SerializeField] AudioSource headAudio;

    bool menuState;
    Vector3[] velocities;
    Vector3[] angularVelocities;
    public bool floppy = false;
    

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        tran = transform;

        //rigid = GetComponentInChildren<Rigidbody>();
        rigids = GetComponentsInChildren<Rigidbody>();

        SkinnedMeshRenderer rend = GetComponentInChildren<SkinnedMeshRenderer>();
        List<Material> mats = new List<Material>();
        rend.GetMaterials(mats);
        for(int i = 0; i < mats.Count; i++)
        {
            Color col = new Color(Random.value, Random.value, Random.value, 1);
            mats[i].color = col;
        }

        foreach(Rigidbody rb in rigids)
        {
            rb.isKinematic = true;
        }
        velocities = new Vector3[rigids.Length];
        angularVelocities = new Vector3[rigids.Length];

        mainParent = tran.parent;
        if(currentSeat != null)
            EnterSeat(currentSeat);

        lastForward = tran.forward;

        wanderPos = tran.position + new Vector3(Random.Range(-50f, 50f), 0f, Random.Range(-50f, 50f));

        menuState = GameControl.instance.inMenu;
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
        headAudio.pitch = Random.Range(0.8f, 2f);
        headAudio.PlayOneShot(GameControl.instance.GetDeathSound());
        GameControl.instance.RemoveFromLiving(this);
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

        foreach (Rigidbody rb in rigids)
        {
            rb.isKinematic = false;
        }
        floppy = true;
    }

    public void Fall()
    {
        if(followRigidbody) return;

        agent.enabled = false;
        anim.enabled = false;
        foreach (Rigidbody rb in rigids)
        {
            rb.isKinematic = false;
        }
        floppy = true;

        followRigidbody = true;

        rigids[0].gameObject.AddComponent<KillOnNextCollide>();

        Scream();

        if(GameControl.instance.autoSlomo && possessed)
            GameControl.instance.SetSlomo(true);

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
        IgnoreMe(seat.GetComponentInParent<Collider>(), true);
        
        /*Collider col = seat.GetComponentInParent<Collider>();
        foreach(Collider c in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(col, c, true);
        }*/
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
        if(Physics.Raycast(head.position, GameControl.instance.followCam.tran.forward, out hit, 3f, seatLayers, QueryTriggerInteraction.Collide))
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
            if(!menuState)
            {
                menuState = true;
                if(floppy)
                {
                    for(int i = 0; i < rigids.Length; i++)
                    {
                        velocities[i] = rigids[i].velocity;
                        angularVelocities[i] = rigids[i].angularVelocity;
                        rigids[i].isKinematic = true;
                    }
                }
            }
            return;
        }
        else
        {
            if(menuState)
            {
                menuState = false;
                if(floppy)
                {
                    for(int i = 0; i < rigids.Length; i++)
                    {
                        rigids[i].isKinematic = false;
                        rigids[i].velocity = velocities[i];
                        rigids[i].angularVelocity = angularVelocities[i];
                    }
                }
            }
        }

        if(!alive) return;

        if (head.position.y < GameControl.instance.waterLevel)
        {
            //if(currentSeat != null)
            //    LeaveSeat();
            Kill();
            return;
        }

        if (onFire)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0)
            {
                Kill();
                return;
            }
        }

        if(currentSeat != null) 
        {
            if(possessed && Input.GetButtonDown("Fire2"))
                LeaveSeat();

            return;
        }

        if(!agent.isOnNavMesh)
            Fall();

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

        if(fleeTimer > 0) fleeTimer -= Time.deltaTime;
        else scared = false;

        if(onFire)
        {
            scared = true;
            fireWanderTimer -= Time.deltaTime;
            if(fireWanderTimer <= 0)
            {
                Scream();
                fireWanderTimer = Random.Range(1f, 5f);
                fleeVector = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
            }
        }

        if(!scared)
        {

            scared = LookForGhost();


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
            if(!onFire)
                LookForGhost();

            if (target != null)
                Flee(target.position);
            else Flee(tran.position + fleeVector * 10);

            if ((agent.destination - tran.position).magnitude > agent.stoppingDistance)
            {
                anim.SetBool("running", true);
                agent.speed = runSpeed;
            }
            else 
            {
                if(!onFire)
                    anim.SetBool("running", false);
                else fireWanderTimer = 0f;
            }
        }

        
    }

    void OnAnimatorMove()
    {
        if (tran.forward != lastForward)
        {
            if (possessed)
            {
                float rotChange = Vector3.SignedAngle(lastForward, tran.forward, tran.up);
                GameControl.instance.followCam.ChangeXRotation(rotChange);
            }

            lastForward = tran.forward;
        }
    }

    void LateUpdate()
    {
        if(tran.forward != lastForward && !anim.enabled)
        {
            if(possessed)
            {
                float rotChange = Vector3.SignedAngle(lastForward, tran.forward, tran.up);
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
            transform.position = rigids[0].position;
            foreach(Transform child in children)
            {
                child.parent = tran;
            }

            if(rigids[0].IsSleeping() && !GameControl.instance.inMenu)
                Kill();
        }
    }

    bool LookForGhost()
    {
        if(GameControl.instance.player == null || GameControl.instance.player.tran.parent != null) return false;

        Vector3 ghostVector = GameControl.instance.player.tran.position - tran.position;
        if(ghostVector.sqrMagnitude <= 20f * 20f)
        {
            if(Vector3.Dot(tran.forward, ghostVector.normalized) > 0.5f)
            {
                fleeVector = ghostVector.normalized;
                fleeTimer = Random.Range(10f, 30f);

                if(!justSawGhost)
                {
                    justSawGhost = true;
                    Scream();
                }
                return true;
            }
        }

        justSawGhost = false;
        return false;
    }

    public void Footstep()
    {
        if(GameControl.instance.inMenu) return;

        footstepAudio.PlayOneShot(GameControl.instance.GetFootstep());
    }

    public void Scream()
    {
        headAudio.pitch = Random.Range(0.8f, 2f);
        headAudio.PlayOneShot(GameControl.instance.GetScreamSound());
    }

    public void IgnoreMe(Collider col, bool setting)
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(col, c, true);
        }
    }
    public void IgnoreSeat(Seat seat)
    {
        foreach(Collider col in seat.root.GetComponentsInChildren<Collider>())
        {
            IgnoreMe(col, true);
        }
    }
}
