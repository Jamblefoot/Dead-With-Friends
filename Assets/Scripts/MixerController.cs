using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MixerController : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] string mixerName;
    public void SetVolume(float sliderValue)
    {
        audioMixer.SetFloat(mixerName, Mathf.Log10(sliderValue) * 20);
    }

    void Start()
    {
        audioMixer.SetFloat(mixerName, Mathf.Log10(GetComponent<Slider>().value) * 20);
    }
}
