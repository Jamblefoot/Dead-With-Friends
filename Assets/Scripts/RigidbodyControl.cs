using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyControl : MonoBehaviour
{
    Rigidbody rigid;
    bool menuState;
    Vector3 velocity;
    Vector3 angularVelocity;
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(rigid == null)
        {
            Destroy(this);
            return;
        }

        if(GameControl.instance.inMenu)
        {
            if(!menuState)
            {
                menuState = true;
                velocity = rigid.velocity;
                angularVelocity = rigid.angularVelocity;
                rigid.isKinematic = true;
            }
        }
        else
        {
            if(menuState)
            {
                menuState = false;
                rigid.isKinematic = false;
                rigid.velocity = velocity;
                rigid.angularVelocity = angularVelocity;
            }
        }
    }
}
