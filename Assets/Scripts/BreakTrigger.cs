using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakTrigger : MonoBehaviour
{
    public Rigidbody[] rigids;

    public GameObject[] objectsToActivate;

    [SerializeField] bool killOnContact = true;
    void OnTriggerEnter(Collider col)
    {
        AICharacter aic = col.GetComponentInParent<AICharacter>();
        bool destroy = false;
        if(aic != null)
        {
            destroy = true;
            if (aic.currentSeat == null && killOnContact)
            {
                aic.Fall();
            }
        }
        if(!destroy && (col.GetComponent<KillTrigger>() || col.GetComponent<Explosion>()))
        {
            destroy = true;
        }

        if(destroy)
        {
            foreach (Rigidbody rb in rigids)
            {
                rb.isKinematic = false;
                if(!rb.GetComponent<RigidbodyControl>())
                    rb.gameObject.AddComponent<RigidbodyControl>();
            }
            foreach (GameObject go in objectsToActivate)
            {
                go.SetActive(true);
            }

            Destroy(gameObject);
        }

    }
}
