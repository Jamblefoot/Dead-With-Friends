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

    Transform mainParent;

    public bool possessed;
    public Transform head;

    public bool dontSpawnGhost = false;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        tran = transform;

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
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }
    void Flee(Vector3 location)
    {
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

    public void EnterSeat(Seat seat)
    {
        if(seat.occupant != null && seat.occupant != this)
            return;
        
        seat.occupant = this;
        agent.enabled = false;
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

        currentSeat = null;
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

        if (tran.position.y < GameControl.instance.waterLevel)
        {
            //if(currentSeat != null)
            //    LeaveSeat();
            Kill();
            return;
        }

        if(currentSeat != null) return;

        if (possessed)
        {
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");

            Seek(tran.position + Vector3.ProjectOnPlane(GameControl.instance.followCam.tran.forward * vertical * 5 + GameControl.instance.followCam.tran.right * horizontal * 5, Vector3.up));

            anim.SetBool("running", false);
            if ((agent.destination - tran.position).magnitude > agent.stoppingDistance)
            {
                anim.SetBool("walking", true);
                agent.speed = 2;
            }
            else anim.SetBool("walking", false);

            return;
        }

        if(!scared)
        {
            if(target != null)
                Seek(target.position);

            if((agent.destination - tran.position).magnitude > agent.stoppingDistance)
            {
                anim.SetBool("walking", true);
                agent.speed = 2;
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
                agent.speed = 10;
            }
            else anim.SetBool("running", false);
        }

        
    }
}
