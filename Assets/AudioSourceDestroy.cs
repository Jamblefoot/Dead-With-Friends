using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceDestroy : MonoBehaviour
{
    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        if(!audioSource.isPlaying)
            Destroy(gameObject);
    }
}
