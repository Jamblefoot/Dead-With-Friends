using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTrigger : MonoBehaviour
{
    CarControl carControl;
    // Start is called before the first frame update
    void Start()
    {
        carControl = GetComponentInParent<CarControl>();
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.GetComponent<CollisionTrigger>())
            carControl.ForceReverse();
    }
}
