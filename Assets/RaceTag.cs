using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTag : MonoBehaviour
{
    public int participantNumber;
    TextMesh[] texts;
    // Start is called before the first frame update
    void Awake()
    {
        texts = GetComponentsInChildren<TextMesh>();
    }

    public void SetNumber(int value)
    {
        participantNumber = value;
        foreach(TextMesh tm in texts)
        {
            tm.text = value.ToString();
        }
    }
}
