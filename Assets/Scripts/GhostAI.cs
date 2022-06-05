using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAI : MonoBehaviour
{
    GhostDrive player;

    public float speedMult = 0.1f;
    public float rotSpeedMult = 5f;

    Transform tran;
    Rigidbody rigid;

    public LayerMask groundLayers;
    public LayerMask barrierLayer;
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
        RaycastHit hit;
        Vector3 barrierPos = new Vector3(tran.position.x, 0, tran.position.z);
        if (Physics.Raycast(Vector3.down * 100, barrierPos.normalized, out hit, barrierPos.magnitude - 1f, barrierLayer, QueryTriggerInteraction.Ignore))
        {
            tran.position = new Vector3(hit.point.x, tran.position.y, hit.point.z);
        }

        if (!Physics.Raycast(tran.position, Vector3.down, Mathf.Infinity, groundLayers, QueryTriggerInteraction.Ignore))
        {
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

        rigid.interpolation = player.rigid.interpolation;
        if(player.possessed != null && player.possessed.currentSeat == null)
            rigid.interpolation = RigidbodyInterpolation.Interpolate;

        tran.LookAt(player.tran, tran.up);

        Vector3 move = Vector3.zero;
        if((player.tran.position - tran.position).sqrMagnitude > followDistance * followDistance)
        {
            move = tran.forward * speedMult * Time.timeScale;
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
