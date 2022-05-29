using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAI : MonoBehaviour
{
    GhostDrive player;

    public float speedMult = 0.1f;
    public float rotSpeedMult = 5f;
    //float acceleration = 10f;
    //float vertCalced = 0f;
    //float horCalced = 0f;
    //float rotSpeed = 0f;

    Transform tran;
    Rigidbody rigid;

    public LayerMask groundLayers;
    public float followDistance = 5f;

    public Texture2D[] faces;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<GhostDrive>();
        tran = transform;
        rigid = GetComponent<Rigidbody>();

        if(faces != null)
        {
            SkinnedMeshRenderer rend = GetComponent<SkinnedMeshRenderer>();
            rend.material.SetTexture("_MainTex", faces[Random.Range(0, faces.Length)]);
        }

        MoveAboveGround();
    }

    void MoveAboveGround()
    {
        if (!Physics.Raycast(tran.position, Vector3.down, Mathf.Infinity, groundLayers, QueryTriggerInteraction.Ignore))
        {
            RaycastHit hit;
            Physics.Raycast(tran.position + Vector3.up * 1000, Vector3.down, out hit, 1000, groundLayers, QueryTriggerInteraction.Ignore);
            tran.position = hit.point + Vector3.up;
        }
    }

    void FixedUpdate()
    {
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

        if(player == null) return;
        if(player.stillAlive) return;

        tran.LookAt(player.tran, tran.up);

        Vector3 move = Vector3.zero;
        if((player.tran.position - tran.position).sqrMagnitude > followDistance * followDistance)
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
