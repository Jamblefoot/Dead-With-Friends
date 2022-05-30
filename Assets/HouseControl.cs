using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseControl : MonoBehaviour
{
    public MeshRenderer[] wallRenderers;

    public Color color = Color.black;

    public GameObject destroyedPrefab;

    public bool blockRecolor = false;

    // Start is called before the first frame update
    void Start()
    {
        if(blockRecolor) return;

        color = new Color(Random.value, Random.value, Random.value, 1);
        foreach(MeshRenderer r in wallRenderers)
        {
            r.material.color = color;
        }
    }

    public void SetColor(Color col)
    {
        color = col;
        foreach (MeshRenderer r in wallRenderers)
        {
            r.material.color = color;
        }

        blockRecolor = true;
    }

    public void Explode()
    {
        if(destroyedPrefab == null) return;

        GameObject dp = Instantiate(destroyedPrefab, transform.position, transform.rotation);
        dp.GetComponent<HouseControl>().SetColor(color);
        Destroy(gameObject);
    }
}
