﻿using UdonSharp;
using UnityEngine;
using System;
using VRC.SDKBase;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(VRC_Pickup))]
public class RobotVacuumScript : UdonSharpBehaviour
{
    public AudioSource audioSource;
    public float MaxSpeed = 0.25f;
    public float Acceleration = 0.2f;
    public float Deceleration = 0.2f;
    public float RotationSpeed = 20.0f;
    public float RotationDegrees = 60.0f;
    [Range(0, 1)]
    public float VolumeEngine = 0.3f;
    public bool Enabled = true;
    public bool MainTriggerSizeDependsOnSpeed = true;
    public BoxCollider MainTrigger;
    public BoxCollider MainTriggerMin;
    public SensorMain sensorMain;
    public ColliderFloor colliderFloor;


    private bool SensorDetected = false;
    public bool sensorDetected { get => SensorDetected; set => SensorDetected = value; }
    private bool OnFloor = false;
    public bool onFloor { get => OnFloor; set => OnFloor = value; }
    private bool isTurning = false;
    [UdonSynced]
    private bool SpiningRightSynced = true;
    private bool SpiningRight = true;
    private bool TriggerBottom = false;
    public bool triggerBottom { get => TriggerBottom; set => TriggerBottom = value; }
    private bool Started = false;
    private float Speed = 0.0f;
    private float Delay = 0.0f;
    public float delay { get => Delay; set => Delay = value; }
    private float rotateY = 0.0f;
    private float DiffMainTriggerCenter;
    private float DiffMainTriggerSize;
    private float SpeedPercentage;
    [UdonSynced]
    private int rotateCountSync = 0;
    private int rotateCount = 0;
    private int forSmartRotate = 4;
    private int isTurningCount = 0;
    private int TurnNegationCount = 4;
    private Animator animator;
    private Rigidbody rigidBody;
    private DateTime dateTimePoint;
    private DateTime dateTimeSystem;
    private VRC_Pickup Vrc_Pickup;
    private Vector3 MainTriggerCenter;
    private Vector3 MainTriggerSize;
    private Vector3 MainTriggerMinCenter;
    private Vector3 MainTriggerMinSize;
    private VRCPlayerApi localPlayer;

    //Random number
    private int RandomSeed = 1;
    private float AddMaxRotate = 0.15f;
    [UdonSynced]
    private int AddRotate = 0;



    void Start()
    {
        if (!Enabled)
        {
            if (audioSource != null) audioSource.enabled = false;
            this.enabled = false;
        }
        if (audioSource != null) audioSource.PlayDelayed(0.1f);

        rigidBody = GetComponent<Rigidbody>();
        dateTimePoint = DateTime.Now;
        Acceleration /= 100;
        Deceleration /= 100;
        Vrc_Pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        animator = GetComponent<Animator>();

        MainTriggerCenter = MainTrigger.center;
        MainTriggerSize = MainTrigger.size;
        MainTriggerMinCenter = MainTriggerMin.center;
        MainTriggerMinSize = MainTriggerMin.size;

        DiffMainTriggerCenter = MainTriggerCenter.y - MainTriggerMinCenter.y;
        DiffMainTriggerSize = MainTriggerSize.y - MainTriggerMinSize.y;

        if (MainTriggerSizeDependsOnSpeed)
        {
            MainTrigger.center = new Vector3(MainTriggerCenter.x, MainTriggerMinCenter.y, MainTriggerCenter.z);
            MainTrigger.size = new Vector3(MainTriggerMinSize.x, MainTriggerMinSize.y, MainTriggerMinSize.z);
        }

        DefaultAudioSource();
    }

    new void OnDeserialization()
    {
        if (SpiningRightSynced != SpiningRight)
            SpiningRight = SpiningRightSynced;

        if (!Started)
        {
            rotateCount = rotateCountSync;
            Started = true;
        }
    }

    private void FixedUpdate()
    {
        if (!Enabled) return;
        //LoopEngineSound();

        if (!Vrc_Pickup.IsHeld)
        {
            if (onFloor)
            {
                if (Vector3.Dot(transform.up, Vector3.up) > 0.9)
                {
                    if (Delay == 0)
                    {
                        if (!isTurning)
                        {
                            SetAlert(false);
                            SpeedPercentage = Speed / MaxSpeed;
                            audioSource.volume = (0.3f + (SpeedPercentage * 0.7f)) * VolumeEngine;
                            audioSource.pitch = 0.5f + (SpeedPercentage * 0.5f);
                            if (MainTriggerSizeDependsOnSpeed) TriggerSize();
                            if (!SensorDetected && TriggerBottom) SpeedAcceleration();
                            else SpeedDeceleration(true);
                        }
                        else
                        {
                            if (rotateCount < forSmartRotate)
                                Rotate();
                            else SmartRotate();
                        }
                    }
                    else //if (Owner())
                    {
                        Delay -= Time.deltaTime;
                        if (Delay < 0)
                        {
                            Delay = 0;
                            SetAlert(false);
                        }
                    }
                }
                else
                {
                    ResetSettings();
                    SpeedDeceleration(false);
                    SetAlert(true);
                }
            }
            else //if (!onFloor)
            {
                SpeedDeceleration(false);
                SetAlert(true);
            }
        }
    }

    private bool Owner() => (Networking.GetOwner(Vrc_Pickup.gameObject) == Networking.LocalPlayer);

    public override void OnPickup()
    {
        SendNetworking(nameof(ResetSettings));
        SendNetworking(nameof(SetAlertTrue));
    }

    public override void OnDrop() => SendNetworking(nameof(ResetFloorDetector));

    public void ResetFloorDetector() => colliderFloor.ResetFloorDetector();

    public void SendNetworking(string str) => SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, str);

    private void TriggerSize()
    {
        if (SpeedPercentage > 0.25f)
        {
            MainTrigger.center = new Vector3(MainTriggerCenter.x, (MainTriggerMinCenter.y + (DiffMainTriggerCenter * SpeedPercentage)), MainTriggerCenter.z);
            MainTrigger.size = new Vector3(MainTriggerMinSize.x, (MainTriggerMinSize.y + (DiffMainTriggerSize * SpeedPercentage)), MainTriggerMinSize.z);
        }
        else
        {
            MainTrigger.center = new Vector3(MainTriggerCenter.x, MainTriggerMinCenter.y, MainTriggerCenter.z);
            MainTrigger.size = new Vector3(MainTriggerMinSize.x, MainTriggerMinSize.y, MainTriggerMinSize.z);
        }
    }


    private void DefaultAudioSource()
    {
        audioSource.pitch = 0.5f;
        audioSource.volume = 0.3f * VolumeEngine;
    }

    public void ResetSettings()
    {
        Speed = 0;

        ResetRotate();

        DefaultAudioSource();
    }

    public void ResetRotate()
    {
        isTurning = false;
        if (Owner()) rotateCountSync = 0;
        rotateCount = 0;
        rotateY = 0;
    }


    private void SpeedAcceleration()
    {
        if (Acceleration + Speed < MaxSpeed)
        {
            Speed += Acceleration;
            rigidBody.MovePosition(transform.position + transform.forward * (Speed * Time.deltaTime));
        }
        else
        {
            Speed = MaxSpeed;
            rigidBody.MovePosition(transform.position + transform.forward * (Speed * Time.deltaTime));
        }
    }

    private void SpeedDeceleration(bool setRotateValues)
    {
        if (Speed - Deceleration > 0)
        {
            Speed -= Deceleration;
            rigidBody.MovePosition(transform.position + transform.forward * (Speed * Time.deltaTime));
        }
        else
        {
            if (setRotateValues)
                SetRotateValues();
            else if (Speed != 0) Speed = 0;
        }
    }

    private void SetRotateValues()
    {
        Speed = 0;
        isTurning = true;
        rotateY = RotationDegrees + AddRotate;

        audioSource.pitch = 0.6f;
        audioSource.volume = 0.4f * VolumeEngine;
    }


    private void Rotate()
    {
        SetAlert(false);
        float rotate = RotationSpeed * Time.deltaTime;
        rotateY -= rotate;
        if (rotateY > 0)
        {
            if (SpiningRight) transform.Rotate(Vector3.up, rotate);
            else transform.Rotate(Vector3.down, rotate);
        }
        else
        {
            if (Owner()) SendNetworking(nameof(StopRotate));
        }
    }

    private void SmartRotate()
    {
        if (SpiningRight) transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
        else transform.Rotate(Vector3.up, -(RotationSpeed * Time.deltaTime));

        if (!sensorDetected && TriggerBottom)
        {
            rotateY = 10;
            if (Owner()) rotateCountSync = 0;
            rotateCount = 0;
        }
    }



    private void RotateIter()
    {
        if (Owner())
        {
            isTurningCount++;
            if (isTurningCount == TurnNegationCount)
            {
                SpiningRightSynced = !SpiningRightSynced;
                SpiningRight = !SpiningRight;
                isTurningCount = 0;
                AddRotate = GenerateRandom();
            }
        }
    }


    private int GenerateRandom()
    {
        int ret = 0;
        if (RandomSeed++ == 100)
            RandomSeed = 1;
        string t = (Mathf.PI * RandomSeed).ToString().Replace(",", "");
        if (t.Length > 6)
            t = t.Substring(3, 2);
        else return 0;
        if (!int.TryParse(t, out ret))
            return 0;
        ret = (int)(ret * AddMaxRotate);
        if (RandomSeed % 2 == 0)
            ret *= -1;
        return ret;
    }

    public void StopRotate()
    {
        if (!sensorDetected && TriggerBottom)
        {
            RotateIter();
            SetAlert(false);
            ResetSettings();
            sensorMain.SensorOff();
        }
        else
        {
            if (Owner()) rotateCountSync++;
            rotateCount++;
            if (rotateCount == forSmartRotate)
            {
                SetAlert(true);
                return;
            }
            SetRotateValues();
        }
    }


    public void SetAlert(bool b) => animator.SetBool("Alert", b);

    public void SetAlertTrue() => animator.SetBool("Alert", true);


    // private void LoopEngineSound(float t) // 5.1f
    // {
    //     dateTimeSystem = DateTime.Now;
    //     if (dateTimeSystem.Subtract(dateTimePoint).TotalSeconds > t)
    //     {
    //         audioSource.time = 0.3f;
    //         dateTimePoint = DateTime.Now;
    //     }
    // }
}
