using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropaneTank : MonoBehaviour
{
    [SerializeField] Mesh explodedMesh;
    [SerializeField] GameObject explosionPrefab;

    MeshFilter meshFilter;
    MeshRenderer rend;

    bool onFire = false;
    float fireTimer = 0f;

    bool exploded = false;
    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        rend = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if(GameControl.instance.inMenu) return;

        //do fire damage here
        if(onFire)
        {
            if(fireTimer < 5f)
            {
                fireTimer += Time.deltaTime;
                rend.material.SetFloat("_Displacement", fireTimer * 0.2f);
            }
            else Explode();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.rigidbody != null)
        {
            if(col.rigidbody.velocity.magnitude > 5f)
                Explode();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        FireTrigger ft = other.GetComponent<FireTrigger>();
        if(ft != null)
        {
            onFire = true;
        }
    }

    public void Explode()
    {
        if(exploded) return;

        meshFilter.mesh = explodedMesh;
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        rend.material.SetFloat("_Displacement", 0f);
        exploded = true;
        Destroy(this);
    }
}
