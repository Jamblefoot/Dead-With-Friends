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
        agent.enabled = false;
        transform.position = transform.position + Vector3.up * 2;
        anim.enabled = false;
        Instantiate(ghostPrefab, transform.position, transform.rotation);

        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!alive) return;

        if (tran.position.y < GameControl.instance.waterLevel)
        {
            Kill();
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
