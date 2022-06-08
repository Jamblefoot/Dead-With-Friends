using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    Transform tran;
    Rigidbody rigid;

    public Transform destination;
    public Waypoint waypoint;
    public bool wander = true;
    public Seat driverSeat;
    [SerializeField] float stoppingDistance = 5f;
    [SerializeField] float maxSpeed = 15f;
    [SerializeField] float acceleration = 50f;
    [SerializeField] float rotSpeed = 5f;

    float currentSpeed = 0f;

    [SerializeField] LayerMask groundLayers;
    [SerializeField] float groundDistance = 2f;

    [SerializeField] Transform[] axles;
    [SerializeField] AudioSource engineAudio;
    [SerializeField] GameObject killbox;

    float vertical, horizontal;

    float stuckTimer;
    float unstickTimer;
    Vector3 lastPos;

    [SerializeField] GameObject smokeParticles;
    [SerializeField] GameObject explosionPrefab;
    float explodeTimer;
    float damage;

    bool menuState;
    Vector3 velocity;
    Vector3 angularVelocity;
    // Start is called before the first frame update
    void Start()
    {
        tran = transform;
        rigid = GetComponent<Rigidbody>();

        rigid.centerOfMass = Vector3.zero;

        MeshRenderer rend = GetComponentInChildren<MeshRenderer>();
        List<Material> mats = new List<Material>();
        rend.GetMaterials(mats);
        mats[0].color = new Color(Random.value, Random.value, Random.value, 1);

        if(wander && waypoint == null)
        {
            waypoint = FindNearestWaypoint();
        }

        lastPos = tran.position;

        menuState = GameControl.instance.inMenu;
    }

    // Update is called once per frame
    void Update()
    {
        float vel = rigid.velocity.magnitude;
        if (vel < 5f)
        {
            killbox.SetActive(false);
        }
        else killbox.SetActive(true);


        if(driverSeat.occupant == null) 
        {
            if(engineAudio.isPlaying)
                engineAudio.Stop();
            return;
        }

        if(driverSeat.occupant.possessed)
        {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
        }
        else
        {
            vertical = 0; 
            horizontal = 0;
        }

        if(!engineAudio.isPlaying)
            engineAudio.Play();
        
        engineAudio.volume = Mathf.Clamp(vel * 10, 0.2f, 1f);
        engineAudio.pitch = Mathf.Clamp(vel / 6, 0.5f, 1.5f);

        
    }

    bool CheckGrounded()
    {
        return Physics.Raycast(tran.position, -tran.up, groundDistance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    void FixedUpdate()
    {


        if (GameControl.instance.inMenu)
        {
            if(menuState != GameControl.instance.inMenu)
            {
                menuState = GameControl.instance.inMenu;
                velocity = rigid.velocity;
                angularVelocity = rigid.angularVelocity;
            }
            rigid.isKinematic = true;
            return;
        }
        else 
        {
            rigid.isKinematic = false;
            if(menuState != GameControl.instance.inMenu)
            {
                menuState = GameControl.instance.inMenu;
                rigid.velocity = velocity;
                rigid.angularVelocity = angularVelocity;
            }
        }

        if (!CheckGrounded()) 
        {
            explodeTimer += Time.deltaTime;

            if(explodeTimer > 10f)
            {
                smokeParticles.SetActive(true);

                if(explodeTimer > 15f)
                {
                    if(driverSeat.occupant != null && Vector3.Dot(tran.up, Vector3.up) < 0f && !driverSeat.occupant.possessed)
                    {
                        AICharacter aic = driverSeat.occupant;
                        driverSeat.occupant.LeaveSeat();
                        aic.RunAwayFrom(transform.position, 5, 15);
                    }

                    if(explodeTimer > 20f)
                    {
                        Explode();
                    }
                }
            }
            
            return;
        }
        else 
        {
            if(damage > 10)
            {
                explodeTimer = 10f;
            }

            if(explodeTimer < 10f)
                explodeTimer = 0;
            else 
            {
                explodeTimer = 10f;
                damage = Mathf.Max(damage, explodeTimer);
            }
        }

        if(damage > 10f)
        {
            if(!smokeParticles.activeSelf)
                smokeParticles.SetActive(true);

            if(damage > 20f)
                Explode();
        }

        if (driverSeat.occupant == null) return;

        if(driverSeat.occupant.possessed)
        {
            MoveTowardPosition(tran.position + tran.forward * vertical * (stoppingDistance + 1) + tran.right * horizontal * (stoppingDistance + 1), Vector3.zero);
            return;
        }

        if (destination != null)
        {
            MoveTowardPosition(destination.position, Vector3.zero);
            return;
        }

        if(waypoint != null)
        {
            if(MoveTowardPosition(waypoint.transform.position, waypoint.plane) < 10f)
            {
                if(wander)
                {
                    waypoint = waypoint.connections[Random.Range(0, waypoint.connections.Length)];
                }
            }
        }

        //FUNCTIONALITY NEEDED
        // Regular driving mode - avoidance of pedestrians and other cars
        // Explode when collide with static object or other car at high enough speed
    }

    float MoveTowardPosition(Vector3 position, Vector3 plane)
    {


        Vector3 dest = position;
        dest.y = 0;
        Vector3 pos = tran.position;
        pos.y = 0;

        float dist = Vector3.Distance(dest, pos);

        int reverseMult = 1;

        if (dist > stoppingDistance)
        {
            float dot = Vector3.Dot(tran.forward, (position - tran.position).normalized);
            if(dot < -0.5f) reverseMult = -1;

            bool forceTurn = false;
            if(unstickTimer > 0f && !driverSeat.occupant.possessed)
            {
                reverseMult = -1;
                unstickTimer -= Time.deltaTime;
                forceTurn = true;
            }

            if (currentSpeed < maxSpeed)
                currentSpeed += acceleration * Time.deltaTime;

            rigid.AddForce(reverseMult * tran.forward * currentSpeed * Time.deltaTime, ForceMode.VelocityChange);

            for (int i = 0; i < axles.Length; i++)
            {
                axles[i].localRotation = axles[i].localRotation * Quaternion.Euler(reverseMult * 5f, 0f, 0f);
            }

            //Vector3 torque = Vector3.Cross(tran.forward, (destination.position - tran.position).normalized);
            //rigid.AddTorque(torque * 100f);
            if(!driverSeat.occupant.possessed)
            {
                Vector3 dir = (position - tran.position).normalized;
                if(forceTurn) dir = tran.right;
                Vector3 rot = Vector3.RotateTowards(tran.forward, dir, Time.deltaTime * rotSpeed, 0);
                float rotChange = Vector3.SignedAngle(tran.forward, rot, tran.up);
                rigid.AddTorque(tran.up * 2000f * rotChange);
            }
            else
            {
                if(Mathf.Abs(horizontal) > 0.01f)
                {
                    Vector3 rot = Vector3.RotateTowards(tran.forward, (position - tran.position).normalized, Time.deltaTime * rotSpeed, 0);
                    float rotChange = Vector3.SignedAngle(tran.forward, rot, tran.up) * reverseMult;
                    rigid.AddTorque(tran.up * 2000f * rotChange);
                }
            }
        }
        else currentSpeed = 0;

        if(unstickTimer <= 0f)
        {
            if(Vector3.Distance(lastPos, tran.position) <= 0.1f)
            {
                if(stuckTimer < 5f)
                    stuckTimer += Time.deltaTime;
                else
                {
                    unstickTimer = 5f;
                    stuckTimer = 0f;
                }
            }
            else 
            {
                if(stuckTimer > 0f)
                    stuckTimer -= Time.deltaTime;
            }
        }
        lastPos = tran.position;

        if(plane != Vector3.zero && Vector3.Dot(Vector3.ProjectOnPlane(position - tran.position, Vector3.up).normalized, tran.TransformVector(plane)) < -0.5f)
            dist = 0f;

        return dist;
    }

    Waypoint FindNearestWaypoint(bool searchInFront = true)
    {
        float dist = 5000;
        Waypoint wp = null;
        foreach(Waypoint w in FindObjectsOfType<Waypoint>())
        {
            float d = Vector3.Distance(tran.position, w.transform.position);
            if(d < dist)
            {
                if(searchInFront)
                {
                    if(Vector3.Dot((w.transform.position - tran.position).normalized, tran.forward) > 0f)
                    {
                        dist = d;
                        wp = w;
                    }
                }
                else
                {
                    dist = d;
                    wp = w;
                }
            }
        }

        if(wp == null && searchInFront)
        {
            FindNearestWaypoint(false);
        }

        return wp;
    }

    public void ForceReverse()
    {
        unstickTimer = 5f;
    }

    void Explode()
    {
        AICharacter aic = null;
        if (driverSeat.occupant != null)
        {
            aic = driverSeat.occupant;
            driverSeat.occupant.LeaveSeat();
        }
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            if (col.GetComponent<Rigidbody>() == null)
            {
                col.transform.parent = null;
                col.gameObject.AddComponent<Rigidbody>();
                col.gameObject.AddComponent<RigidbodyControl>();
            }

            if(aic != null) aic.IgnoreMe(col, true);
        }
        engineAudio.Stop();
        Instantiate(explosionPrefab, tran.position, Quaternion.identity);
        gameObject.AddComponent<RigidbodyControl>();
        Destroy(this);
    }

    public void AddDamage(int amount)
    {
        damage += amount;
    }
}
