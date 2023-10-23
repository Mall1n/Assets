using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


public class ButtonSwitch : UdonSharpBehaviour
{

    public GameObject[] TargetSwitch = null;

    public bool Enabled = true;
    private GameObject slider = null;

    [Header("Enable sync buttons between other in canvas (not sync with other players)")]

    public bool SyncButtonsEnabled = false;
    public Transform[] SyncButtons = null;


    [Header("Enable conflicts")]

    public bool Conflict = false;
    public Transform[] ObjectsConflict = null;


    [Header("Sync with tablet")]

    public bool SyncTablet = false;
    public UdonBehaviour UdonTabletButton = null;



    [ColorUsage(true, true)]
    private Color ColorLight;
    [ColorUsage(true, true)]
    private Color ColorDark;
    private UdonBehaviour UdonColorController;

    void Start()
    {
        slider = transform.Find("Slider").gameObject;
        Enabled = slider.activeSelf;
        UdonColorController = (UdonBehaviour)transform.root.GetComponent(typeof(UdonBehaviour));
        if (UdonColorController != null)
        {
            ColorLight = (Color)UdonColorController.GetProgramVariable("colorEnabled");
            ColorDark = (Color)UdonColorController.GetProgramVariable("colorDisabled");
        }
    }

    //public void ChangeBool() => Enabled = !Enabled;


    void Interact()
    {
        FullChange();
    }

    public void FullChange()
    {
        MainChange();

        if (SyncButtonsEnabled && SyncButtons != null) Sync_Buttons();

        if (Conflict && ObjectsConflict != null) Objects_Conflict();

        if (SyncTablet && UdonTabletButton != null) Sync_Tablet();
    }

    public void ChangeWithoutSync()
    {
        MainChange();

        if (SyncButtonsEnabled && SyncButtons != null) Sync_Buttons();

        if (Conflict && ObjectsConflict != null) Objects_Conflict();
    }

    public void MainChange()
    {
        if (Enabled)
            ChangeColor(this.transform, ColorDark);
        else
            ChangeColor(this.transform, ColorLight);
        for (int i = 0; i < TargetSwitch.Length; i++)
            if (TargetSwitch[i] != null)
                TargetSwitch[i].SetActive(!Enabled);
        if (slider != null) slider.SetActive(!Enabled);
        Enabled = !Enabled;
    }

    void Sync_Buttons()
    {
        for (int i = 0; i < SyncButtons.Length; i++)
            if (SyncButtons[i] != null)
            {
                UdonBehaviour u = (UdonBehaviour)SyncButtons[i].transform.Find("Button").GetComponent(typeof(UdonBehaviour));
                u.SendCustomEvent("ChangeWithoutSync");
            }
    }

    void Objects_Conflict()
    {
        for (int i = 0; i < ObjectsConflict.Length; i++)
            if (ObjectsConflict[i] != null && ObjectsConflict[i].transform.Find("Button/Slider").gameObject.activeSelf)
            {
                UdonBehaviour u = (UdonBehaviour)ObjectsConflict[i].transform.Find("Button").GetComponent(typeof(UdonBehaviour));
                u.SendCustomEvent("MainChange");
            }
    }

    void Sync_Tablet()
    {
        UdonBehaviour udon = (UdonBehaviour)UdonTabletButton.GetProgramVariable("UdonLerp");
        bool left = (bool)udon.GetProgramVariable("left");
        if (!left != Enabled)
            UdonTabletButton.SendCustomEvent("ChangeWithoutSync");
    }

    void ChangeColor(Transform t, Color color)
    {
        ColorBlock colorBlock = t.GetComponent<Button>().colors;
        colorBlock.normalColor = color;
        t.GetComponent<Button>().colors = colorBlock;
    }



}


