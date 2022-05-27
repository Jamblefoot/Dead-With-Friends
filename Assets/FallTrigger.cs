using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        AICharacter aic = col.GetComponentInParent<AICharacter>();
        if (aic != null && aic.alive && aic.currentSeat == null)
        {
            aic.Fall();
        }
    }
}
