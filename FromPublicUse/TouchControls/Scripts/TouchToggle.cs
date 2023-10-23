using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

using UnityEngine.UI;

public class TouchToggle : UdonSharpBehaviour
{
    [Tooltip("Flag to specify that the state of this control should be consistent across all clients. Also affects whether the event forwarding variable is networked.")]
    public bool syncControlState = false;
    [Tooltip("Flag to specify whether to start the control as ON or OFF.")]
    public bool initialState = false;
    [Tooltip("If the flag is enabled, it means that only the master can interact with this object. If synchronization between clients is disabled, then this flag will be false during compilation")]
    public bool onlyMaster = false;
    [Space]

    [Header("Toggle State Indicators")]
    [Tooltip("The object to show when the control state is OFF.")]
    public GameObject toggleOffObject = null;
    [Tooltip("The object to show when the control state is ON.")]
    public GameObject toggleOnObject = null;
    [Tooltip("An optional custom object that will be shown only when the player is within the culling distance of either type (pointer or touch). The 'activeSelf' value will be modified on this object.")]
    public GameObject tooltip;
    [Space]

    [Header("Object Toggling")]
    [Tooltip("This flag determines whether or not to enforce linking the visibility state of the objects with the control being on/off or just swap based on the state they are already in.")]
    public bool linkObjectStates = false;
    [Tooltip("Optional list of objects that will be made visible/hidden along with the control's state (modifies the 'activeSelf' state on the object).")]
    public GameObject[] objectsToToggle = null;
    [Space]

    [Header("VR Touch Mode Settings")]
    [Tooltip("The intensity of the vibration in the controllers when activating the control in Touch Mode.")]
    [Range(0.0f, 1.0f)] public float hapticStrength = 1.0f;
    [Tooltip("The duration of the vibration in the controllers when activating the control in Touch Mode.")]
    [Range(0.0f, 1.0f)] public float hapticDuration = 0.2f;
    [Tooltip("This flag tells the script to only use the normal Interact method instead of the Touch Mode method when in VR. Does not affect desktop mode.")]
    public bool disableTouchModeInVR = false;
    // [Tooltip("This flag specifies whether to use a physical push of the button, or just detect if the hand is within the button (aka Normal Touch Mode)")]
    // public bool useTactileTouchMode = false;
    [Tooltip("This value specifies how close the player's hand must be to the control to check for the physical hand detection in Touch Mode. Helps with performance. 0.15 is usually a good value, so this typically doesn't need changed.")]
    public float touchModeCullingDistance = 0.15f;
    [Tooltip("This value specifies how close the player's hand must be to the control to check for the pointer detection in Interact Mode. 1.5 is usually a good value, so this typically doesn't need changed.")]
    public float pointerCullingDistance = 1.5f;
    [Space]

    [Header("Event Forwarding (read the tooltips)")]
    [Tooltip("Custom udon component to receive an event when control is interacted with. Event forwarding will be skipped if left empty.")]

    public UdonBehaviour udonTarget;
    [Tooltip("Optional variable name that will have the true/false value of the state that the control has moved to. This is set immediately before sending the event.")]
    public string variableNameTarget;
    [Tooltip("Event to call when control is interacted with. If no name is provided, it will default to 'OnChange'.")]
    public string onChangeEvent = "OnChange";
    [Tooltip("Enabling this flag will cause the event to be sent to all clients in the instance instead of just the one who triggered it (aka global vs local). This can cause unexpected behavior if you don't know what you are doing, so be careful when enabling this setting.")]
    public bool onChangeIsNetworked = false;
    [Tooltip("This flag determines that when the events are sent, they have the current state's numerical index appended. Eg: if you specify the Event Name as 'MyEvent', the control will call 'MyEvent0' or 'MyEvent1' depending on which state the control has moved to. The 0 suffix means the control is OFF and 1 means it is ON.")]
    public bool sendNumberedEvents = false;

    [Header("Change variable")]
    public bool ChangeVariable = false;
    public int SendId = -1;

    [Header("Conflict Objects")]
    public bool Conflict = false;
    public UdonBehaviour udon = null;

    [UdonSynced] private bool syncState = true;
    private bool currentState = true;
    private const float vibAmplitudeCoefficient = 0.128f;
    private Collider interactCollider;
    private Collider areaCollider;
    private Collider pushCollider;
    private LineRenderer pointerLine;
    private bool rightTouchState = false;
    private bool leftTouchState = false;
    private bool skipLog = true;

    private void log(string value)
    {
        if (!skipLog) Debug.Log("[<color=#00ffcc>TouchToggle</color>] " + gameObject.name + ": " + value);
    }

    void Start()
    {
        if (syncControlState == false || onChangeIsNetworked == false) 
            onlyMaster = false;
            
        currentState = initialState;
        if (syncControlState && Networking.IsMaster)
        {
            syncState = currentState;
        }

        if (tooltip == gameObject) tooltip = null; // clear default value
        interactCollider = transform.Find("InteractTrigger").GetComponent<Collider>();
        areaCollider = transform.Find("AreaTrigger").GetComponent<Collider>();
        // pushCollider = transform.Find("PushTrigger").GetComponent<Collider>();
        pointerLine = interactCollider.GetComponent<LineRenderer>();
        pointerLine.enabled = false;
        if (toggleOffObject == null && toggleOnObject == null)
        {
            log("NOTICE: No game object states were provided");
        }
        if (linkObjectStates) updateObjectToggles();
        updateIndicators();
        log("Started");
    }

    // public void ChangeState()
    // {
    //     currentState = !currentState;
    //     if (syncControlState)
    //     {
    //         Networking.SetOwner(Networking.LocalPlayer, gameObject);
    //         syncState = currentState;
    //     }
    // }

    void CheckConflict()
    {
        if (Conflict && udon != null && !currentState)
        {
            var left = udon.GetProgramVariable("syncState");
            if (left.GetType() == typeof(bool) && (bool)left == false)
                udon.SendCustomEvent("ActivateWOSync");
        }
    }

    // Use these three events to swap between modes on-demand.
    // Can be useful for people who have unusual avatars and need to change mode for ease of use.

    public void UseInteractMode()
    {
        disableTouchModeInVR = true;
        // useTactileTouchMode = false;
        log("Switching to Interact Mode");
    }

    public void UseAreaMode()
    {
        // desktop cannot use Touch Modes, skip
        if (!Networking.LocalPlayer.IsUserInVR()) return;
        disableTouchModeInVR = false;
        // useTactileTouchMode = false;
        log("Switching to Normal Touch Mode");
    }

    // public void UsePushMode()
    // {
    //     // desktop cannot use Touch Modes, skip
    //     if (!Networking.LocalPlayer.IsUserInVR()) return;
    //     disableTouchModeInVR = false;
    //     // useTactileTouchMode = true;
    //     log("Switching to Tactile Touch Mode");
    // }

    new void OnDeserialization()
    {
        if (syncControlState && currentState != syncState)
        {
            currentState = syncState;
            updateObjectToggles();
            updateIndicators();
            forwardVariable();
            if (onChangeIsNetworked) forwardOnChangeWOSync();
        }

    }

    private void activateWithHaptics(VRC_Pickup.PickupHand hand)
    {
        Networking.LocalPlayer.PlayHapticEventInHand(hand, hapticDuration, hapticStrength * vibAmplitudeCoefficient, 320.0f);
        Activate();
    }

    public void Activate()
    {
        currentState = !currentState;
        if (syncControlState)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            syncState = currentState;
        }
        updateIndicators();
        updateObjectToggles();
        forwardVariable();
        forwardOnChange();
        CheckConflict();
    }

    public void ActivateWOSync()
    {
        currentState = !currentState;
        if (syncControlState)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            syncState = currentState;
        }
        updateIndicators();
        updateObjectToggles();
        forwardVariable();
        forwardOnChangeWOSync();
    }

    private void updateIndicators()
    {
        // do not allow script to toggle it's own object off. stupid default values...
        if (toggleOffObject != null && toggleOffObject != gameObject) toggleOffObject.SetActive(!currentState);
        if (toggleOnObject != null && toggleOffObject != gameObject) toggleOnObject.SetActive(currentState);
    }

    private void updateObjectToggles()
    {
        if (objectsToToggle != null)
            for (int i = 0; i < objectsToToggle.Length; i++)
                // do not allow script to toggle it's own object off. stupid default values...
                if (objectsToToggle[i] != null && objectsToToggle[i] != gameObject)
                    objectsToToggle[i].SetActive(linkObjectStates ? currentState : !objectsToToggle[i].activeSelf);
    }

    private void forwardVariable()
    {
        if (udonTarget != null && udonTarget != this && variableNameTarget != null)
            udonTarget.SetProgramVariable(variableNameTarget, currentState);
    }

    private void forwardOnChange()
    {
        if (udonTarget != null && udonTarget != this)
        {
            if (onChangeEvent == null) onChangeEvent = "OnChange";
            if (sendNumberedEvents) onChangeEvent += currentState ? "1" : "0";
            if (ChangeVariable)
            {
                udonTarget.SetProgramVariable("Id", SendId);
            }
            Networking.SetOwner(Networking.LocalPlayer, udonTarget.gameObject);
            udonTarget.SendCustomEvent(onChangeEvent);
        }
    }

    private void forwardOnChangeWOSync()
    {
        if (udonTarget != null && udonTarget != this)
        {
            if (onChangeEvent == "") return;
            if (ChangeVariable)
            {
                udonTarget.SetProgramVariable("Id", SendId);
            }
            Networking.SetOwner(Networking.LocalPlayer, udonTarget.gameObject);
            udonTarget.SendCustomEvent("ChangeWithoutSync");
        }
    }

    void FixedUpdate()
    {
        var local = Networking.LocalPlayer;
        if (local == null) return;
        if (!onlyMaster || (onlyMaster && Networking.IsMaster))
        {
            if (disableTouchModeInVR || !local.IsUserInVR())
            {
                detectRaycastCollision();
            }
            else
            {
                // if (useTactileMode) {}
                // else
                detectHandCollision();
            }
        }

    }

    private void detectRaycastCollision()
    {
        var local = Networking.LocalPlayer;
        RaycastHit hit;
        if (local.IsUserInVR())
        {
            var haptic = VRC_Pickup.PickupHand.Right;
            var hand = local.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            var pointer = new Ray(hand.position, hand.rotation * (Vector3.forward + new Vector3(0.8f, 0f, 0f)));
            var button = "Oculus_CrossPlatform_SecondaryIndexTrigger";
            if (!interactCollider.Raycast(pointer, out hit, pointerCullingDistance))
            {
                haptic = VRC_Pickup.PickupHand.Left;
                hand = local.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                pointer = new Ray(hand.position, hand.rotation * (Vector3.forward + new Vector3(0.8f, 0f, 0f)));
                button = "Oculus_CrossPlatform_PrimaryIndexTrigger";
            }
            if (interactCollider.Raycast(pointer, out hit, pointerCullingDistance))
            {
                pointerLine.enabled = true;
                pointerLine.SetPosition(0, hand.position);
                pointerLine.SetPosition(1, hit.point);
                if (tooltip != null && !tooltip.activeSelf) tooltip.SetActive(true);
                if (Input.GetAxisRaw(button) > 0.98f) 
                {
                    if (rightTouchState == true) return;
                    rightTouchState = true;
                    activateWithHaptics(haptic);
                }
                else rightTouchState = false;
            }
            else
            {
                if (tooltip != null && tooltip.activeSelf) tooltip.SetActive(false);
                pointerLine.enabled = false;
                rightTouchState = false;
            }
        }
        else
        {
            if (pointerLine.enabled) pointerLine.enabled = false;
            var button = "Fire1";
            var head = local.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            var pointer = new Ray(head.position, head.rotation * Vector3.forward);
            if (interactCollider.Raycast(pointer, out hit, pointerCullingDistance))
            {
                if (tooltip != null && !tooltip.activeSelf) tooltip.SetActive(true);
                if (Input.GetButton(button))
                {
                    if (rightTouchState == true) return;
                    rightTouchState = true;
                    Activate();
                }
                else rightTouchState = false;
            }
            else
            {
                if (tooltip != null && tooltip.activeSelf) tooltip.SetActive(false);
                rightTouchState = false;
            }
        }
    }


    private void detectHandCollision()
    {
        var local = Networking.LocalPlayer;
        // for efficiency, the order of the bones are sorted from most likely to be used to least likely to be used.
        // due to udon limitations, the Bone references cannot be put a class field or returned from a separate method.
        var boneWithinCull = false;
        for (var i = 0; i < 4; i++)
        {
            HumanBodyBones bone;
            switch (i)
            {
                case 0: bone = HumanBodyBones.RightIndexDistal; break;
                case 1: bone = HumanBodyBones.RightIndexIntermediate; break;
                case 2: bone = HumanBodyBones.RightIndexProximal; break;
                default: bone = HumanBodyBones.RightHand; break;
            }
            var pos = local.GetBonePosition(bone);
            if (pos == Vector3.zero) continue; // skip bones that don't exist
            if (areaCollider.gameObject.activeSelf == false) continue;
            var closest = areaCollider.ClosestPoint(pos);
            if (Vector3.Distance(closest, pos) > touchModeCullingDistance)
                break; // hand is not close enough to care about checking all points. skip hand.
            boneWithinCull = true;
            if (tooltip != null && !tooltip.activeSelf) tooltip.SetActive(true);
            if (closest == pos)
            {
                if (rightTouchState == true) break;
                rightTouchState = true;
                activateWithHaptics(VRC_Pickup.PickupHand.Right);
                return;
            }
            else if (i == 3) rightTouchState = false;
        }
        for (var i = 0; i < 4; i++)
        {
            HumanBodyBones bone;
            switch (i)
            {
                case 0: bone = HumanBodyBones.LeftIndexDistal; break;
                case 1: bone = HumanBodyBones.LeftIndexIntermediate; break;
                case 2: bone = HumanBodyBones.LeftIndexProximal; break;
                default: bone = HumanBodyBones.LeftHand; break;
            }
            var pos = local.GetBonePosition(bone);
            if (pos == Vector3.zero) continue; // skip bones that don't exist
            if (areaCollider.gameObject.activeSelf == false) continue;
            var closest = areaCollider.ClosestPoint(pos);
            if (Vector3.Distance(closest, pos) > touchModeCullingDistance)
                break; // hand is not close enough to care about checking all points. skip hand.
            boneWithinCull = true;
            if (tooltip != null && !tooltip.activeSelf) tooltip.SetActive(true);
            if (closest == pos)
            {
                if (leftTouchState == true) break;
                leftTouchState = true;
                activateWithHaptics(VRC_Pickup.PickupHand.Left);
                return;
            }
            else if (i == 3) leftTouchState = false;
        }
        if (tooltip != null && !boneWithinCull && tooltip.activeSelf)
            tooltip.SetActive(false);
    }
}
