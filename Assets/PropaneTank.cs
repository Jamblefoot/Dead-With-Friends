using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropaneTank : MonoBehaviour
{
    [SerializeField] Mesh explodedMesh;
    [SerializeField] GameObject explosionPrefab;

    MeshFilter meshFilter;
    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    void Update()
    {
        //do fire damage here
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.rigidbody != null)
        {
            if(col.rigidbody.velocity.magnitude > 5f)
                Explode();
        }
    }

    void Explode()
    {
        meshFilter.mesh = explodedMesh;
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        Destroy(this);
    }
}
