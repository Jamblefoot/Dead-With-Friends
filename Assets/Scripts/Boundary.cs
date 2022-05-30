using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    MeshRenderer rend;
    [SerializeField] float displayTime = 2f;

    float displayTimer = 0f;

    void Start()
    {
        rend = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if(displayTimer > 0)
        {
            displayTimer -= Time.deltaTime;
            rend.enabled = true;
            rend.material.SetFloat("_Alpha", displayTimer / (displayTime * 2f));// * 2 is to keep it at half alpha max
        }
        else
        {
            rend.enabled = false;
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if(col.collider.GetComponent<GhostDrive>())
        {
            displayTimer = displayTime;
        }
    }
}
