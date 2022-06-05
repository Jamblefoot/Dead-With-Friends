using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemDestroy : MonoBehaviour
{
    [SerializeField] bool unparent = false;
    ParticleSystem particles;
    // Start is called before the first frame update
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        if(unparent) 
            transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameControl.instance.inMenu)
        {
            if(!particles.isPaused)
                particles.Pause();
            return;
        }
        else
        {
            if(particles.isPaused)
                particles.Play();
        }

        if(!particles.isPlaying)
            Destroy(gameObject);
    }
}
