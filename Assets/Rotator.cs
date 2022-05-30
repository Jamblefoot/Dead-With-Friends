using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    float rotSpeed = 100f;
    int rotAxis = 2;

    Transform tran;
    // Start is called before the first frame update
    void Start()
    {
        tran = transform;
    }


    void FixedUpdate()
    {
        if(GameControl.instance.inMenu) return;

        switch(rotAxis)
        {
            case 0://x axis
                tran.localRotation = tran.localRotation * Quaternion.Euler(rotSpeed * Time.deltaTime, 0f, 0f);
                break;
            case 1://y axis
                tran.localRotation = tran.localRotation * Quaternion.Euler(0f, rotSpeed * Time.deltaTime, 0f);
                break;
            case 2://z axis
                tran.localRotation = tran.localRotation * Quaternion.Euler(0f, 0f, rotSpeed * Time.deltaTime);
                break;
        }
    }
}
