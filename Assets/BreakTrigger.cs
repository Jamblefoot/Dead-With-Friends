using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakTrigger : MonoBehaviour
{
    public Rigidbody[] rigids;
    void OnTriggerEnter(Collider col)
    {
        AICharacter aic = col.GetComponentInParent<AICharacter>();
        if(aic != null)
        {
            foreach(Rigidbody rb in rigids)
            {
                rb.isKinematic = false;
            }
        }
    }
}
