using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAI : MonoBehaviour
{
    Transform player;

    public float speedMult = 0.1f;
    public float rotSpeedMult = 5f;
    float acceleration = 10f;
    float vertCalced = 0f;
    float horCalced = 0f;
    float rotSpeed = 0f;

    Transform tran;
    Rigidbody rigid;

    public LayerMask groundLayers;
    public float followDistance = 5f;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<GhostDrive>().transform;
        tran = transform;
        rigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {

        if(player == null) return;

        tran.LookAt(player, tran.up);

        Vector3 move = Vector3.zero;
        if((player.position - tran.position).sqrMagnitude > followDistance * followDistance)
        {
            move = tran.forward * speedMult;
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
}