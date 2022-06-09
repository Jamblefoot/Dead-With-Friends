using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCheckpoint : MonoBehaviour
{
    public int number;

    RaceControl raceControl;
    // Start is called before the first frame update
    void Start()
    {
        raceControl = GetComponentInParent<RaceControl>();
    }

    void OnTriggerEnter(Collider col)
    {
        RaceTag tag = col.GetComponentInChildren<RaceTag>();
        if(tag != null)
        {
            raceControl.AddPoint(tag.participantNumber, number);
        }
    }
}
