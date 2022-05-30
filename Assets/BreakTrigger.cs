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
        if(aic != null)
        {
            foreach(Rigidbody rb in rigids)
            {
                rb.isKinematic = false;
            }
            foreach(GameObject go in objectsToActivate)
            {
                go.SetActive(true);
            }

            if(aic.currentSeat == null && killOnContact)
            {
                aic.Fall();
            }

            Destroy(gameObject);
        }
    }
}
