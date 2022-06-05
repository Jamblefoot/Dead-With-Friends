using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float force = 800f;

    void OnTriggerExit(Collider col)
    {
        if(!col.isTrigger)
            Explode();
    }

    void Explode()
    {
        Explosion exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity).GetComponent<Explosion>();
        exp.explosionForce = force;
        Destroy(gameObject);
    }
}
