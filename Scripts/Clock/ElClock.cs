using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ElClock : UdonSharpBehaviour
{

    [Range(1.05f, 10.0f)]
    public float AnimationSpeed = 5.0f;

    public GameObject[] DaysOfWeek = new GameObject[7];

    private SkinnedMeshRenderer leftSecond;
    private SkinnedMeshRenderer rightSecond;

    private SkinnedMeshRenderer leftMinute;
    private SkinnedMeshRenderer rightMinute;

    private SkinnedMeshRenderer leftHour;
    private SkinnedMeshRenderer rightHour;

    private SkinnedMeshRenderer dot;

    private DateTime currentTime;
    private int second;
    private int minute;
    private int hour;
    private float shapevalue = 0;

    void Start()
    {
        currentTime = DateTime.Now;

        //currentTime = currentTime.AddSeconds(82800 - 20);

        leftSecond = transform.Find("SecondLeftPoint/SecondLeft").GetComponent<SkinnedMeshRenderer>();
        rightSecond = transform.Find("SecondRightPoint/SecondRight").GetComponent<SkinnedMeshRenderer>();

        leftMinute = transform.Find("MinuteLeftPoint/MinuteLeft").GetComponent<SkinnedMeshRenderer>();
        rightMinute = transform.Find("MinuteRightPoint/MinuteRight").GetComponent<SkinnedMeshRenderer>();

        leftHour = transform.Find("HourLeftPoint/HourLeft").GetComponent<SkinnedMeshRenderer>();
        rightHour = transform.Find("HourRightPoint/HourRight").GetComponent<SkinnedMeshRenderer>();

        dot = transform.Find("GapPoint/Gap").GetComponent<SkinnedMeshRenderer>();

        second = currentTime.Second;
        minute = currentTime.Minute;
        hour = currentTime.Hour;

        SetTime(leftSecond, rightSecond, second);
        SetTime(leftMinute, rightMinute, minute);
        SetTime(leftHour, rightHour, hour);

        DaysOfWeek[4].SetActive(false);
        SetDayOfWeek();
    }

    void SetTime(SkinnedMeshRenderer leftmesh, SkinnedMeshRenderer rightmesh, int time)
    {
        int left = (int)decimal.Floor(time / 10);
        int right = time % 10;

        leftmesh.SetBlendShapeWeight(left, 100);
        rightmesh.SetBlendShapeWeight(right, 100);
    }

    private void Update()
    {
        currentTime = DateTime.Now;
        //currentTime = currentTime.AddMilliseconds(1000 / 90);

        second = currentTime.Second;
        int left = (int)decimal.Floor(second / 10);
        int right = second % 10;

        shapevalue = currentTime.Millisecond / 10 * AnimationSpeed;
        if (shapevalue > 100) shapevalue = 100;

        if (second % 2 == 0)
        {
            dot.SetBlendShapeWeight(0, shapevalue);
        }
        else
        {
            dot.SetBlendShapeWeight(0, 100 - shapevalue);
        }

        if (right == 0)
        {
            rightSecond.SetBlendShapeWeight(9, 100 - shapevalue);
            rightSecond.SetBlendShapeWeight(right, shapevalue);

            if (left == 0)
            {
                leftSecond.SetBlendShapeWeight(5, 100 - shapevalue);
                leftSecond.SetBlendShapeWeight(left, shapevalue);
                NextMinute();
            }
            else
            {
                leftSecond.SetBlendShapeWeight(left - 1, 100 - shapevalue);
                leftSecond.SetBlendShapeWeight(left, shapevalue);
            }
        }
        else
        {
            rightSecond.SetBlendShapeWeight(right - 1, 100 - shapevalue);
            rightSecond.SetBlendShapeWeight(right, shapevalue);
        }
    }

    void NextMinute()
    {
        minute = currentTime.Minute;
        int left = (int)decimal.Floor(minute / 10);
        int right = minute % 10;

        if (right == 0)
        {
            rightMinute.SetBlendShapeWeight(9, 100 - shapevalue);
            rightMinute.SetBlendShapeWeight(right, shapevalue);

            if (left == 0)
            {
                leftMinute.SetBlendShapeWeight(5, 100 - shapevalue);
                leftMinute.SetBlendShapeWeight(left, shapevalue);
                NextHour();
            }
            else
            {
                leftMinute.SetBlendShapeWeight(left - 1, 100 - shapevalue);
                leftMinute.SetBlendShapeWeight(left, shapevalue);
            }
        }
        else
        {
            rightMinute.SetBlendShapeWeight(right - 1, 100 - shapevalue);
            rightMinute.SetBlendShapeWeight(right, shapevalue);
        }
    }

    void NextHour()
    {
        hour = currentTime.Hour;
        int left = (int)decimal.Floor(hour / 10);
        int right = hour % 10;

        if (right == 0)
        {
            rightHour.SetBlendShapeWeight(9, 100 - shapevalue);
            rightHour.SetBlendShapeWeight(right, shapevalue);

            if (left == 0)
            {
                leftHour.SetBlendShapeWeight(2, 100 - shapevalue);
                leftHour.SetBlendShapeWeight(left, shapevalue);
                SetDayOfWeek();
                rightHour.SetBlendShapeWeight(3, 100 - shapevalue);
            }
            else
            {
                leftHour.SetBlendShapeWeight(left - 1, 100 - shapevalue);
                leftHour.SetBlendShapeWeight(left, shapevalue);
            }
        }
        else
        {
            rightHour.SetBlendShapeWeight(right - 1, 100 - shapevalue);
            rightHour.SetBlendShapeWeight(right, shapevalue);
        }
    }



    void SetDayOfWeek()
    {
        switch (currentTime.DayOfWeek.ToString())
        {
            case "Monday":
                DaysOfWeek[6].SetActive(false);
                DaysOfWeek[0].SetActive(true);
                break;
            case "Tuesday":
                DaysOfWeek[0].SetActive(false);
                DaysOfWeek[1].SetActive(true);
                break;
            case "Wednesday":
                DaysOfWeek[1].SetActive(false);
                DaysOfWeek[2].SetActive(true);
                break;
            case "Thursday":
                DaysOfWeek[2].SetActive(false);
                DaysOfWeek[3].SetActive(true);
                break;
            case "Friday":
                DaysOfWeek[3].SetActive(false);
                DaysOfWeek[4].SetActive(true);
                break;
            case "Saturday":
                DaysOfWeek[4].SetActive(false);
                DaysOfWeek[5].SetActive(true);
                break;
            case "Sunday":
                DaysOfWeek[5].SetActive(false);
                DaysOfWeek[6].SetActive(true);
                break;
            default:
                break;
        }
    }
}
