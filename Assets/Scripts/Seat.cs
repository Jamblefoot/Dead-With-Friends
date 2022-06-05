using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat : MonoBehaviour
{
    public Transform root;
    public AICharacter occupant;

    void Start()
    {
        if (root == null)
        {
            Rigidbody rb = GetComponentInParent<Rigidbody>();
            if(rb != null)
                root = rb.transform;
        }
    }
}


