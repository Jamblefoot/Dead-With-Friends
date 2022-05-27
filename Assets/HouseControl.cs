using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseControl : MonoBehaviour
{
    public MeshRenderer[] wallRenderers;
    // Start is called before the first frame update
    void Start()
    {
        Color col = new Color(Random.value, Random.value, Random.value, 1);
        foreach(MeshRenderer r in wallRenderers)
        {
            r.material.color = col;
        }
    }
}
