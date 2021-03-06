using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float explosionForce = 1500f;

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
            float force = explosionForce;
            if(col.attachedRigidbody.mass > 100f)//is a car
            {
                force = col.attachedRigidbody.mass * (explosionForce * 0.5f);
            }
            col.attachedRigidbody.AddExplosionForce(force, transform.position, 10, 0.5f, ForceMode.Force);

            return;
        }

        CarControl cc = col.GetComponent<CarControl>();
        if(cc != null)
        {
            cc.AddDamage(10);
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
