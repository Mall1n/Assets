using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

public class TimeLocal : UdonSharpBehaviour
{

    private Text time = null;
    private Text pmam = null;
    private DateTime dateTime;
    private int Minutes = -1;

    void Start()
    {
        time = transform.Find("Time").GetComponent<Text>();
        pmam = transform.Find("pmam").GetComponent<Text>();
    }

    private void OnEnable() => Minutes = -1;

    private void Update() 
    {
        dateTime = DateTime.Now;
        if (dateTime.Minute != Minutes)
        {
            string t = "";
            if (dateTime.Hour > 11)
            {
                pmam.text = "pm";
                time.text = $"{dateTime.Hour - 12}:{(t = dateTime.Minute.ToString().Length == 1 ? $"0{dateTime.Minute.ToString()}" : dateTime.Minute.ToString())}";
            }
            else
            {
                pmam.text = "am";
                time.text = $"{dateTime.Hour}:{(t = dateTime.Minute.ToString().Length == 1 ? $"0{dateTime.Minute.ToString()}" : dateTime.Minute.ToString())}";
            }
            Minutes = dateTime.Minute;
        }
    }
}
