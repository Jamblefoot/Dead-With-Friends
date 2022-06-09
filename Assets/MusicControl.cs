using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicControl : MonoBehaviour
{
    [SerializeField] AudioClip mainSong;
    [SerializeField] AudioClip altSong;
    AudioSource audioSource;

    float time = 0;
    float altTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(audioSource.clip == mainSong)
        {
            if(Time.timeScale < 1f)// && GameControl.instance.player.possessed // && GameControl.instance.player.possessed.floppy)
            {
                time = audioSource.time;
                audioSource.Stop();
                audioSource.clip = altSong;
                
                audioSource.Play();
                audioSource.time = altTime;
            }
        }
        else
        {
            if(Time.timeScale >= 1f)
            {
                altTime = audioSource.time;
                audioSource.Stop();
                audioSource.clip = mainSong;
                audioSource.Play();
                audioSource.time = time;
            }
        }
    }
}
