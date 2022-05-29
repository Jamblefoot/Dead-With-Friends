using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnNextCollide : MonoBehaviour
{

    void OnCollisionEnter(Collision col)
    {
        if(col.transform.root != transform.root)
        {
            GetComponentInParent<AICharacter>().Kill();
            Destroy(this);
        }
    }
}
