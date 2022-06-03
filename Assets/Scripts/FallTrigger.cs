using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    [Tooltip("Vector relative to fall trigger transform to push AICharacter head")]
    public Vector3 pushVector = Vector3.zero;

    void OnTriggerEnter(Collider col)
    {
        AICharacter aic = col.GetComponentInParent<AICharacter>();
        if (aic != null && aic.alive && aic.currentSeat == null)
        {
            aic.Fall();
            aic.head.GetComponentInChildren<Rigidbody>().AddForce(transform.TransformVector(pushVector) * Time.deltaTime, ForceMode.Force);
        }
    }
}
