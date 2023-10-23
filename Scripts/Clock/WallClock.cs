
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

public class WallClock : UdonSharpBehaviour
{

    public Transform ArrowSecond;
    public Transform ArrowMinute;
    public Transform ArrowHour;
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    [Range(0.0f, 1.0f)]
    public float Volume = 0.6f;

    private DateTime currentTime;
    private int secondsNow = -1;


    void Start()
    {
        if (audioSource == null)
        {
            Transform Sound = transform.Find("Sound");
            if (Sound != null)
                audioSource = Sound.GetComponent<AudioSource>();
        }
        if (ArrowSecond == null) ArrowSecond = transform.Find("ArrowSecond");
        if (ArrowMinute == null) ArrowMinute = transform.Find("ArrowMinute");
        if (ArrowHour == null) ArrowHour = transform.Find("ArrowHour");
        audioClips = Resize(audioClips);
        if (audioSource == null && ArrowSecond == null &&
        ArrowMinute == null && ArrowHour == null && audioClips.Length == 0) enabled = false;
    }

    AudioClip[] Resize(AudioClip[] m)
    {
        AudioClip[] temp;
        int counter = 0;
        for (int i = 0; i < m.Length; i++)
            if (m[i] != null)
                counter++;
        temp = new AudioClip[counter];
        counter = 0;
        for (int i = 0; i < m.Length; i++)
            if (m[i] != null)
            {
                temp[counter] = m[i];
                counter++;
            }
        return temp;
    }

    void FixedUpdate()
    {
        currentTime = DateTime.Now;

        if (secondsNow != currentTime.Second)
        {
            secondsNow = currentTime.Second;
            NextSecond();
        }
    }

    void NextSecond()
    {
        float second = (float)currentTime.Second / 60 * 360;
        float minute = (float)currentTime.Minute / 60 * 360 + second / 60;
        float hour = (float)currentTime.Hour / 12 * 360 + minute / 12;

        if (ArrowSecond != null)
            ArrowSecond.localRotation = Quaternion.Euler(0, 0, second);
        if (ArrowMinute != null)
            ArrowMinute.localRotation = Quaternion.Euler(0, 0, minute);
        if (ArrowHour != null)
            ArrowHour.localRotation = Quaternion.Euler(0, 0, hour);
        if (audioSource != null || audioClips.Length != 0)
        {
            audioSource.clip = audioClips[(int)Random(0, audioClips.Length)];
            audioSource.pitch = 1.0f + Random(-0.1f, 0.1f);
            audioSource.Play();
        }
    }

    float Random(float i, float j) { return (float)UnityEngine.Random.Range(i, j); }
}
