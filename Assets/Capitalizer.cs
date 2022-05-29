using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Capitalizer : MonoBehaviour
{
    InputField input;
    void Start()
    {
        input = GetComponent<InputField>();
    }

    public void Capitalize()
    {
        string s = input.text;
        s.ToUpper();
        if(s != input.text)
            input.text = s;
    }
}
