using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    Transform tran;
    Rigidbody rigid;

    public Transform destination;
    public Seat driverSeat;
    float stoppingDistance = 5f;
    float maxSpeed = 15f;
    float acceleration = 5f;
    float rotSpeed = 5f;

    float currentSpeed = 0f;

    public LayerMask groundLayers;

    public Transform[] axles;

    float vertical, horizontal;
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
    }

    // Update is called once per frame
    void Update()
    {
        if(driverSeat.occupant == null) return;

        if(driverSeat.occupant.possessed)
        {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
        }
        else
        {
            vertical = 0; horizontal = 0;
        }
        
    }

    bool CheckGrounded()
    {
        return Physics.Raycast(tran.position, -tran.up, 1f, groundLayers, QueryTriggerInteraction.Ignore);
    }

    void FixedUpdate()
    {
        if (GameControl.instance.inMenu)
        {
            rigid.isKinematic = true;
            return;
        }
        else rigid.isKinematic = false;

        if (!CheckGrounded()) return;
        if (driverSeat.occupant == null) return;

        if(driverSeat.occupant.possessed)
        {
            MoveTowardPosition(tran.position + tran.forward * vertical * -10 + tran.right * horizontal * -10);
            return;
        }

        if (destination != null)
        {
            MoveTowardPosition(destination.position);
            
        }

        //FUNCTIONALITY NEEDED
        // Regular driving mode - avoidance of pedestrians and other cars
        // Reverse when stuck on something
        // Explode when collide with static object or other car at high enough speed
    }

    void MoveTowardPosition(Vector3 position)
    {
        Vector3 dest = position;
        dest.y = 0;
        Vector3 pos = tran.position;
        pos.y = 0;

        float dist = Vector3.Distance(dest, pos);

        if (dist > stoppingDistance)
        {
            if (currentSpeed < maxSpeed)
                currentSpeed += acceleration * Time.deltaTime;

            rigid.AddForce(-tran.forward * currentSpeed * Time.deltaTime, ForceMode.VelocityChange);

            for (int i = 0; i < axles.Length; i++)
            {
                axles[i].localRotation = axles[i].localRotation * Quaternion.Euler(-5f, 0f, 0f);
            }

            //Vector3 torque = Vector3.Cross(tran.forward, (destination.position - tran.position).normalized);
            //rigid.AddTorque(torque * 100f);
            Vector3 rot = Vector3.RotateTowards(-tran.forward, (position - tran.position).normalized, Time.deltaTime * rotSpeed, 0);
            float rotChange = Vector3.SignedAngle(-tran.forward, rot, tran.up);
            rigid.AddTorque(tran.up * 2000f * rotChange);
        }
    }
}
