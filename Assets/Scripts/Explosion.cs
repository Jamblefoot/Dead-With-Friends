using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 0.5f);
    }

    void OnTriggerEnter(Collider col)
    {
        AICharacter aic = col.GetComponentInParent<AICharacter>();
        if(aic != null)
        {
            if(aic.currentSeat != null)
            {
                aic.IgnoreSeat(aic.currentSeat);
                aic.LeaveSeat();
            }
            aic.Fall();
        }
        if(col.attachedRigidbody != null && !col.attachedRigidbody.isKinematic)
        {
            float force = 1200f;
            if(col.attachedRigidbody.mass > 100f)//is a car
            {
                force = col.attachedRigidbody.mass * 1000;
            }
            col.attachedRigidbody.AddExplosionForce(force, transform.position, 10, 0.5f, ForceMode.Force);

            return;
        }

        HouseControl hc = col.GetComponentInParent<HouseControl>();
        if(hc != null)
        {
            hc.Explode();
            return;
        }

        PropaneTank pt = col.GetComponent<PropaneTank>();
        if(pt != null) 
        {
            pt.Explode();
        }
    }
}
