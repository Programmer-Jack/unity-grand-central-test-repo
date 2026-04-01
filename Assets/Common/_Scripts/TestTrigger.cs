using System;
using UnityEngine;

public class TestTrigger : MonoBehaviour
{
    private AudioSource _audio;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _audio.Play();
    }
}
