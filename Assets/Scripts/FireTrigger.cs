using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrigger : MonoBehaviour
{
    [SerializeField] GameObject firePrefab;

    Transform tran;

    public bool destroyingSelf;
    // Start is called before the first frame update
    void Start()
    {
        tran = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(tran.position.y < GameControl.instance.waterLevel)
        {
            destroyingSelf = true;
            AICharacter aic = tran.parent.GetComponentInParent<AICharacter>();
            if(aic != null)
            {
                bool noLongerOnFire = true;
                foreach(FireTrigger f in aic.GetComponentsInChildren<FireTrigger>())
                {
                    if(!f.destroyingSelf)
                        noLongerOnFire = false;
                }
                if(noLongerOnFire)
                    aic.onFire = false;
            }
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        AICharacter aic = col.GetComponentInParent<AICharacter>();
        if(aic != null)
        {
            if(!aic.onFire)
            {
                aic.onFire = true;
                aic.fireTimer = Random.Range(30f, 60f);
            }
            if(col.GetComponentInChildren<FireTrigger>())
                return;

            GameObject fire = Instantiate(firePrefab, col.transform.position, col.transform.rotation, col.transform);
            fire.transform.localScale = Vector3.one;
            BoxCollider bc = fire.GetComponent<BoxCollider>();
            if(col.GetComponent<BoxCollider>())
            {
                BoxCollider colBC = col.GetComponent<BoxCollider>();
                bc.size = colBC.size * 1.1f;
                bc.center = colBC.center;
                return;
            }
            else if(col.GetComponent<SphereCollider>())
            {
                SphereCollider sph = col.GetComponent<SphereCollider>();
                float size = sph.radius * 2.1f;
                bc.size = new Vector3(size, size, size);
                bc.center = sph.center;
                return;
            }
            else if(col.GetComponent<CapsuleCollider>())
            {
                CapsuleCollider cap = col.GetComponent<CapsuleCollider>();
                Vector3 size = new Vector3(cap.radius * 2.1f, cap.radius * 2.1f, cap.radius * 2.1f);
                switch(cap.direction)
                {
                    case 0://x alignment
                        size.x = Mathf.Max(cap.height, size.x);
                        break;
                    case 1://y alignment
                        size.y = Mathf.Max(cap.height, size.y);
                        break;
                    case 2://z alignment
                        size.z = Mathf.Max(cap.height, size.z);
                        break;
                }
                bc.size = size;
                bc.center = cap.center;
            }
        }
    }
}
