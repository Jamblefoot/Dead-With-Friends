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
            if(col.GetComponent<BoxCollider>())
            {
                fire.GetComponent<BoxCollider>().size = col.GetComponent<BoxCollider>().size * 1.1f;
                return;
            }
            else if(col.GetComponent<SphereCollider>())
            {
                float size = col.GetComponent<SphereCollider>().radius * 2;
                fire.GetComponent<BoxCollider>().size = new Vector3(size, size, size);
                return;
            }
            else if(col.GetComponent<CapsuleCollider>())
            {
                CapsuleCollider cap = col.GetComponent<CapsuleCollider>();
                Vector3 size = Vector3.zero;
                switch(cap.direction)
                {
                    case 0://x alignment
                        size.x = Mathf.Max(cap.height, cap.radius * 2);
                        size.y = cap.radius * 2;
                        size.z = cap.radius * 2;
                        break;
                    case 1://y alignment
                        size.y = Mathf.Max(cap.height, cap.radius * 2);
                        size.x = cap.radius * 2;
                        size.z = cap.radius * 2;
                        break;
                    case 2://z alignment
                        size.z = Mathf.Max(cap.height, cap.radius * 2);
                        size.y = cap.radius * 2;
                        size.x = cap.radius * 2;
                        break;
                }
                fire.GetComponent<BoxCollider>().size = size;
            }
        }
    }
}
