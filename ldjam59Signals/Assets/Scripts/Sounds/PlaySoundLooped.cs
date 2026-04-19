using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlaySoundLooped : MonoBehaviour
{
    public float periodFrom;
    public float periodTo;
    public List<AudioClip> clips;
    public AudioSource AudioSource;

    private void Start()
    {
        StartCoroutine(PlayRoutine());
    }

    public IEnumerator PlayRoutine()
    {
        while (true)
        {
            AudioSource.clip = clips[Random.Range(0, clips.Count)];
            AudioSource.Play();
            yield return new WaitForSeconds(Random.Range(periodFrom, periodTo));
        }
    }
}
