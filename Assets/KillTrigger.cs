using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    public bool dontKillSeated = false;

    void OnTriggerEnter(Collider col)
    {
        AICharacter aic = col.GetComponentInParent<AICharacter>();
        if(aic != null && aic.alive)
        {
            if(dontKillSeated)
            {
                if(aic.currentSeat == null)
                    aic.Kill();
            }
            else aic.Kill();
        }
    }
}
