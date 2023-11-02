using UdonSharp;
using UnityEngine;
using System;
using VRC.SDKBase;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(VRC_Pickup))]

public class VacuumV2 : UdonSharpBehaviour
{
    public AudioSource audioSource;
    public float MaxSpeed = 0.25f;
    public float Acceleration = 0.2f;
    public float Deceleration = 0.2f;
    public float RotationSpeed = 20.0f;
    public float RotationPitDegrees = 39.0f;
    [Range(0, 1)]
    public float VolumeEngine = 0.3f;
    public bool Enabled = true;
    //public GameObject MainTrigger;
    //private BoxCollider MainTriggerCollider;
    public MainTrigger mainTrigger;
    public TriggerFloor colliderFloor;
    // public BoxCollider triggerBottomLeft;
    // public BoxCollider triggerBottomRight;
    // public BoxCollider triggerTopLeft;
    // public BoxCollider triggerTopRight;

    // public bool triggerBottomLeft { get => triggerBottomLeft; set => triggerBottomLeft = value; }
    // public bool triggerBottomRight { get => triggerBottomRight; set => triggerBottomRight = value; }
    // public bool triggerTopLeft { get => triggerTopLeft; set => triggerTopLeft = value; }
    // public bool triggerTopRight { get => triggerTopRight; set => triggerTopRight = value; }

    public bool triggerBottomLeft;
    public bool triggerBottomRight;
    public bool BothPits;
    public bool triggerTopLeft;
    public bool triggerTopRight;


    public bool MainTrigger = false; // private
    public bool _MainTrigger { get => MainTrigger; set => MainTrigger = value; }
    private bool OnFloor = false;
    public bool onFloor { get => OnFloor; set => OnFloor = value; }
    public bool rotateDegrees = false; //private
    public bool Turning = false; // private
    public bool turningLeftSide = false; // private
    //public bool leftTrigger { get => leftTrigger; set => leftTrigger = value; }
    //[UdonSynced]
    //private bool SpiningRightSynced = true;
    //private bool SpiningRight = true;
    //public bool triggerBottom { get => triggerBottom; set => triggerBottom = value; }

    //public bool triggerTop { get => triggerTop; set => triggerTop = value; }
    //private bool Started = false;
    private float Speed = 0.0f;
    private float Delay = 0.0f;
    public float delay { get => Delay; set => Delay = value; }
    public float rotateNeed = 0.0f; // private
    private float SpeedPercentage;
    //[UdonSynced]
    //private int rotateCountSync = 0;
    //private int rotateCount = 0;
    //private int forSmartRotate = 4;
    //private int isTurningCount = 0;
    //private int TurnNegationCount = 4;
    private Animator animator;
    private Rigidbody rigidBody;
    private DateTime dateTimePoint;
    //private DateTime dateTimeSystem;
    private VRC_Pickup Vrc_Pickup;
    private VRCPlayerApi localPlayer;

    // Random number
    // private int RandomSeed = 1;
    // private float AddMaxRotate = 0.15f;
    // [UdonSynced]
    // private int AddRotate = 0;



    void Start()
    {
        if (!Enabled)
        {
            if (audioSource != null) audioSource.enabled = false;
            this.enabled = false;
        }
        if (audioSource != null) audioSource.PlayDelayed(0.5f);
        DefaultAudioSource();

        rigidBody = GetComponent<Rigidbody>();
        dateTimePoint = DateTime.Now;
        MaxSpeed /= 100;
        Acceleration /= 1000;
        Deceleration /= 1000;
        Vrc_Pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        animator = GetComponent<Animator>();

    }

    //new void OnDeserialization()
    //{
    // if (SpiningRightSynced != SpiningRight)
    //     SpiningRight = SpiningRightSynced;

    // if (!Started)
    // {
    //     //rotateCount = rotateCountSync;
    //     Started = true;
    // }
    //}

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
                        SetAlert(false);
                        SpeedPercentage = Speed / MaxSpeed;
                        audioSource.volume = (0.3f + (SpeedPercentage * 0.7f)) * VolumeEngine;
                        audioSource.pitch = 0.5f + (SpeedPercentage * 0.5f);
                        if (!MainTrigger && /*!BothPits*/ triggerBottomLeft && triggerBottomRight) SpeedAcceleration();
                        else SpeedDeceleration();


                        if (!rotateDegrees)
                        {
                            // if (triggerBottom & (triggerTopLeft || triggerTopRight))
                            // {
                            //     SetRotate(false, RotationEndDegrees);
                            // }
                            //else 
                            if (Turning)
                            {
                                Rotate();
                            }
                            else if (!triggerBottomLeft || !triggerBottomRight)
                            {
                                if (!triggerBottomLeft) SetRotate(false, RotationPitDegrees);
                                else if (!triggerBottomRight) SetRotate(true, RotationPitDegrees);
                            }


                            // else if (triggerTopLeft || triggerTopRight)
                            // {
                            //     if (triggerTopRight)
                            //     {
                            //         turningLeftSide = true;
                            //         Rotate();
                            //     }
                            //     else if (triggerTopLeft)
                            //     {
                            //         turningLeftSide = false;
                            //         Rotate();
                            //     }
                            // }
                        }
                        else RotateDegrees();


                        //if (!triggerBottomLeft || triggerTopLeft) Rotate(false);
                        //else if (!triggerBottomRight || triggerTopRight) Rotate(true);
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
                    SpeedDeceleration();
                    SetAlert(true);
                }
            }
            else //if (!onFloor)
            {
                SpeedDeceleration();
                SetAlert(true);
            }
        }
    }

    //private bool Owner() => (Networking.GetOwner(Vrc_Pickup.gameObject) == Networking.LocalPlayer);

    public override void OnPickup()
    {
        SendNetworking(nameof(ResetSettings));
        SendNetworking(nameof(SetAlertTrue));
    }

    public override void OnDrop() => SendNetworking(nameof(ResetFloorDetector));

    public void ResetFloorDetector() => colliderFloor.ResetFloorDetector();

    public void SendNetworking(string str) => SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, str);


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
        rotateDegrees = false;
        // if (Owner()) rotateCountSync = 0;
        // rotateCount = 0;
        rotateNeed = 0;
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

    private void SpeedDeceleration()
    {
        if (Speed - Deceleration > 0)
        {
            Speed -= Deceleration;
            rigidBody.MovePosition(transform.position + transform.forward * (Speed * Time.deltaTime));
        }
        else
        {
            // if (setRotateValues)
            //     SetRotateValues();
            // else 
            if (Speed != 0) Speed = 0;
        }
    }

    // private void SetRotateValues()
    // {
    //     Speed = 0;
    //     isTurning = true;
    //     rotateY = RotationDegrees + AddRotate;

    //     audioSource.pitch = 0.6f;
    //     audioSource.volume = 0.4f * VolumeEngine;
    // }

    private void SetRotate(bool left, float rotate)
    {
        turningLeftSide = left;
        Debug.Log($"Setrotate left = {turningLeftSide}");
        rotateNeed = rotate;
        rotateDegrees = true;
    }

    private void Rotate()
    {
        float rotate = RotationSpeed * Time.deltaTime;
        if (turningLeftSide) transform.Rotate(Vector3.down, rotate);
        else transform.Rotate(Vector3.up, rotate);

        if (!BothPits && triggerBottomLeft && triggerBottomRight && !triggerTopLeft && !triggerTopRight)
        {
            Debug.Log("Rotate Stop");
            SetRotate(turningLeftSide, 2.5f);
            Turning = false;
        }
    }

    private void RotateDegrees()
    {
        SetAlert(false);
        float rotate = RotationSpeed * Time.deltaTime;
        //transform.Rotate(Vector3.down, rotate);
        // if (left) transform.Rotate(Vector3.down, rotate);
        // else transform.Rotate(Vector3.up, rotate);



        if (rotateNeed - rotate >= 0)
        //if (rotateNeed > 0)
        {
            if (turningLeftSide) transform.Rotate(Vector3.down, rotate);
            else transform.Rotate(Vector3.up, rotate);
            rotateNeed -= rotate;
        }
        else
        {
            rotate = rotateNeed;
            if (turningLeftSide) transform.Rotate(Vector3.down, rotate);
            else transform.Rotate(Vector3.up, rotate);
            //if (Owner()) SendNetworking(nameof(StopRotate));
            ResetRotate();
        }
    }


    // public void StopRotate()
    // {
    //     if (!MainTrigger && triggerBottom)
    //     {
    //         //RotateIter();
    //         SetAlert(false);
    //         ResetSettings();
    //         mainTrigger.SensorOff();
    //     }
    //     else
    //     {
    //         //if (Owner()) rotateCountSync++;
    //         //rotateCount++;
    //         //if (rotateCount == forSmartRotate)
    //         //{
    //         //SetAlert(true);
    //         //return;
    //         //}
    //         //SetRotateValues();
    //     }
    // }

    // private void SmartRotate()
    // {
    //     if (SpiningRight) transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
    //     else transform.Rotate(Vector3.up, -(RotationSpeed * Time.deltaTime));

    //     if (!sensorDetected && triggerBottom)
    //     {
    //         rotateY = 10;
    //         if (Owner()) rotateCountSync = 0;
    //         rotateCount = 0;
    //     }
    // }



    // private void RotateIter()
    // {
    //     if (Owner())
    //     {
    //         isTurningCount++;
    //         if (isTurningCount == TurnNegationCount)
    //         {
    //             //SpiningRightSynced = !SpiningRightSynced;
    //             SpiningRight = !SpiningRight;
    //             isTurningCount = 0;
    //             AddRotate = GenerateRandom();
    //         }
    //     }
    // }


    // private int GenerateRandom()
    // {
    //     int ret = 0;
    //     if (RandomSeed++ == 100)
    //         RandomSeed = 1;
    //     string t = (Mathf.PI * RandomSeed).ToString().Replace(",", "");
    //     if (t.Length > 6)
    //         t = t.Substring(3, 2);
    //     else return 0;
    //     if (!int.TryParse(t, out ret))
    //         return 0;
    //     ret = (int)(ret * AddMaxRotate);
    //     if (RandomSeed % 2 == 0)
    //         ret *= -1;
    //     return ret;
    // }




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
