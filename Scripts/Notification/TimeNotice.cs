using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

public class TimeNotice : UdonSharpBehaviour
{

    public Text[] TextTime = new Text[1];

    private float[] timeOfNotify = new float[1];
    private int Length = 0;
    private int Count = 0;
    private int Seconds = -1;


    private DateTime dateTime;

    private void Start() 
    {
        Length = TextTime.Length;
        timeOfNotify = new float[Length];
    }
    public void JoinAndLeftPlayer()
    {
        Count++;
        if (Count > Length) Count--;
        TextTime = ChangeArray(TextTime, Count);
        timeOfNotify = ChangeArrayFloat(timeOfNotify, Count);
        timeOfNotify[0] = 0.0f;
        TextTime[0].text = "Now";
    }



    void FixedUpdate() 
    {
        //dateTime = dateTime.AddSeconds(1.0f);
        dateTime = DateTime.Now;
        if (dateTime.Second != Seconds)
        {
            for (int i = 0; i < Count; i++)
            {
                timeOfNotify[i] += 1.0f;
                if (timeOfNotify[i] < 60)
                {
                    TextTime[i].text = $"now";
                }
                else if (timeOfNotify[i] < 120)
                {
                    TextTime[i].text = $"{(int)(timeOfNotify[i] / 60)} minute ago";
                }
                else if (timeOfNotify[i] < 3600)
                {
                    TextTime[i].text = $"{(int)(timeOfNotify[i] / 60)} minutes ago";
                }
                else if (timeOfNotify[i] < 7200)
                {
                    TextTime[i].text = $"{(int)(timeOfNotify[i] / 60 / 60)} hour ago";
                }
                else if (timeOfNotify[i] < 3600 * 12)
                {
                    TextTime[i].text = $"{(int)(timeOfNotify[i] / 60 / 60)} hours ago";
                }
                else
                    TextTime[i].text = $"a long time ago";
            }
            Seconds = dateTime.Second;
        }

    }


    Text[] ChangeArray(Text[] inp, int n)
    {
        Text[] temp = (Text[]) inp.Clone();
        temp[0] = inp[n - 1];
        for (int i = 1; i < n; i++)
        {
            temp[i] = inp[i - 1];
        }
        return temp;
    }

    float[] ChangeArrayFloat(float[] inp, int n)
    {
        float[] temp = (float[]) inp.Clone();
        temp[0] = inp[n - 1];
        for (int i = 1; i < n; i++)
        {
            temp[i] = inp[i - 1];
        }
        return temp;
    }
}
