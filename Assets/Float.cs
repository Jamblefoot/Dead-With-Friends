using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    //GameControl gameControl;
    Rigidbody rigid;
    Transform tran;
    // Start is called before the first frame update
    void Start()
    {
        //gameControl = FindObjectOfType<GameControl>();
        rigid = GetComponent<Rigidbody>();
        tran = transform;

        bool deleteMe = false;
        /*if (gameControl == null)
        {
            Debug.Log("float script can't find no GameControl script to reference for water level! Gonna self destruct.");
            deleteMe = true;
        }*/
        if(rigid == null)
        {
            Debug.Log("You hack, there's no rigidbody for the float script to influence on "+ gameObject.name +"! Self destructing immediately.");
            deleteMe = true;
        }

        if(deleteMe) Destroy(this);
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(tran.position.y < GameControl.instance.waterLevel)
            rigid.AddForce(-Physics.gravity * 1.5f, ForceMode.Acceleration);
    }
}
