using UdonSharp;
using UnityEngine;
using System;
using VRC.SDKBase;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(VRC_Pickup))]

public class VacuumV2 : UdonSharpBehaviour
{
    public AudioSource audioSource;
    public float MaxSpeed = 0.25f;
    public float Acceleration = 0.2f;
    public float Deceleration = 0.2f;
    public float RotationSpeed = 25.0f;
    public float RotationAfterObstacle = 10.0f;
    [Range(0, 1)]
    public float VolumeEngine = 0.3f;
    [Range(4, 25)]
    public float timeForRotate = 7.0f;
    public bool Enabled = true;
    public MainTrigger mainTrigger;
    public TriggerFloor colliderFloor;

    private bool TriggerBottomLeft;
    public bool triggerBottomLeft { get => TriggerBottomLeft; set => TriggerBottomLeft = value; }
    private bool TriggerBottomRight;
    public bool triggerBottomRight { get => TriggerBottomRight; set => TriggerBottomRight = value; }
    private bool BothPits;
    public bool bothPits { get => BothPits; set => BothPits = value; }
    private bool TriggerTopLeft;
    public bool triggerTopLeft { get => TriggerTopLeft; set => TriggerTopLeft = value; }
    private bool TriggerTopRight;
    public bool triggerTopRight { get => TriggerTopRight; set => TriggerTopRight = value; }


    private float timeForRotate_State = 0;
    private float RotationSpeed_State = 0;
    private bool MainTrigger = false; // private
    public bool _MainTrigger { get => MainTrigger; set => MainTrigger = value; }
    private bool OnFloor = false;
    public bool onFloor { get => OnFloor; set => OnFloor = value; }
    private bool rotateDegrees = false; //private
    private bool Turning = false; // private
    public bool turning { get => Turning; set => Turning = value; }
    private bool TurningLeftSide = false; // private
    public bool turningLeftSide { get => TurningLeftSide; set => TurningLeftSide = value; }
    private bool FreeRotating = false;
    private float timeInDirectPath = 0.0f; // private
    private float Speed = 0.0f;
    private float Delay = 0.0f;
    public float delay { get => Delay; set => Delay = value; }
    private float rotateNeed = 0.0f; // private
    private float SpeedPercentage;
    private Animator animator;
    private Rigidbody rigidBody;
    private DateTime dateTimePoint;
    private VRC_Pickup Vrc_Pickup;
    private VRCPlayerApi localPlayer;

    [UdonSynced]
    private float SyncRandom = 1;
    private float Random = 1;


    void Start()
    {
        if (!Enabled)
        {
            if (audioSource != null) audioSource.enabled = false;
            this.enabled = false;
        }
        if (audioSource != null) audioSource.PlayDelayed(0.5f);
        DefaultAudioSource();


        timeForRotate_State = timeForRotate;
        RotationSpeed_State = RotationSpeed;
        rigidBody = GetComponent<Rigidbody>();
        dateTimePoint = DateTime.Now;
        MaxSpeed /= 100;
        Acceleration /= 1000;
        Deceleration /= 1000;
        Vrc_Pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        animator = GetComponent<Animator>();

    }

    new void OnDeserialization()
    {
        if (Random != SyncRandom)
            Random = SyncRandom;
    }

    private void initRandom()
    {
        if (Owner())
        {
            SyncRandom = UnityEngine.Random.Range(0.5f, 2.0f);
            Random = SyncRandom;
        }
        //RequestSerialization();
    }

    private void FixedUpdate()
    {
        if (!Enabled) return;

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

                        if (!MainTrigger && TriggerBottomLeft && TriggerBottomRight) SpeedAcceleration();
                        else SpeedDeceleration();

                        if (!rotateDegrees)
                        {
                            if (Turning)
                            {
                                Rotate();
                            }
                            else if (Speed == MaxSpeed)
                            {
                                timeInDirectPath += Time.deltaTime;
                                if (timeInDirectPath > timeForRotate)
                                {
                                    SetRotate((int)(Random * 100) % 2 == 0, RotationAfterObstacle * Random * 2f + 5);
                                    timeInDirectPath = 0.0f;
                                    timeForRotate = timeForRotate_State * Random;
                                    FreeRotating = true;
                                    RotationSpeed *= 0.5f;
                                }
                            }
                        }
                        else RotateDegrees();
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
        else SetAlert(true);
    }

    public void EventRotate()
    {
        timeInDirectPath = 0.0f;
        Turning = true;
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


    private void DefaultAudioSource()
    {
        audioSource.pitch = 0.5f;
        audioSource.volume = 0.3f * VolumeEngine;
    }

    public void ResetSettings()
    {
        Speed = 0;

        StopRotate();

        DefaultAudioSource();
    }

    public void StopFreeRorating()
    {
        if (FreeRotating == true)
        {
            FreeRotating = false;
            RotationSpeed = RotationSpeed_State;
        }
    }

    public void StopRotate()
    {
        rotateDegrees = false;
        rotateNeed = 0;

        StopFreeRorating();

        initRandom();
    }


    private void SetRotate(bool left, float rotate)
    {
        TurningLeftSide = left;
        rotateNeed = rotate;
        rotateDegrees = true;
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
        timeInDirectPath = 0.0f;
        if (Speed - Deceleration > 0)
        {
            Speed -= Deceleration;
            rigidBody.MovePosition(transform.position + transform.forward * (Speed * Time.deltaTime));
        }
        else if (Speed != 0) Speed = 0;
    }



    private void Rotate()
    {
        float rotate = RotationSpeed * Time.deltaTime;
        if (TurningLeftSide) transform.Rotate(Vector3.down, rotate);
        else transform.Rotate(Vector3.up, rotate);

        if (!BothPits && TriggerBottomLeft && TriggerBottomRight && !TriggerTopLeft && !TriggerTopRight)
        {
            SetRotate(TurningLeftSide, RotationAfterObstacle);
            Turning = false;
        }
    }

    private void RotateDegrees()
    {
        SetAlert(false);
        float rotate = RotationSpeed * Time.deltaTime;

        if (rotateNeed - rotate >= 0)
        {
            if (TurningLeftSide) transform.Rotate(Vector3.down, rotate);
            else transform.Rotate(Vector3.up, rotate);
            rotateNeed -= rotate;
        }
        else
        {
            rotate = rotateNeed;
            if (TurningLeftSide) transform.Rotate(Vector3.down, rotate);
            else transform.Rotate(Vector3.up, rotate);
            StopRotate();
        }
    }


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


}
