using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class TimeController : UdonSharpBehaviour
{

    public Text[] times = null;
    public Text[] pm = null;

    private DateTime dateTime;
    private TimeZoneInfo localZone;
    private int minute = -1;
    private int UtcHours;

    //private void Start() => dateTime = DateTime.Now;

    private void OnEnable() => minute = -1;
    void Update()
    {
        dateTime = DateTime.Now;
        //dateTime = dateTime.AddMinutes(1);
        if (minute != dateTime.Minute)
        {
            string t = "";
            DateTime time = dateTime;
            TimeZoneInfo timeZoneInfo;
            for (int i = 0; i < times.Length; i++)
            {
                switch (i)
                {
                    case 1: 
                        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"); //New-York
                        time = TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
                        break;
                    case 2: 
                        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"); //Berlin
                        time = TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
                        break;
                    default: 
                        break;
                }
                if (time.Hour > 11)
                {
                    pm[i].text = "pm";
                    times[i].text = $"{time.Hour - 12}:{(t = time.Minute.ToString().Length == 1 ? $"0{time.Minute.ToString()}" : time.Minute.ToString())}";
                }
                else
                {
                    pm[i].text = "am";
                    times[i].text = $"{time.Hour}:{(t = time.Minute.ToString().Length == 1 ? $"0{time.Minute.ToString()}" : time.Minute.ToString())}";
                }
            }
            minute = dateTime.Minute;
        }
    }
}
