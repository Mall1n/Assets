using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SwitchObjectsOnOff : UdonSharpBehaviour
{
    public GameObject[] TargetSwitch = null;
    public bool Enabled = true;

    private Text text = null;

    [ColorUsage(true, true)]
    private Color ColorLight;
    [ColorUsage(true, true)]
    private Color ColorDark;


    [Header("Sync with tablet")]

    public bool SyncTablet = false;
    public UdonBehaviour UdonTabletButton = null;


    void Start()
    {
        text = transform.GetComponent<Text>();
        UdonBehaviour UdonColorController = (UdonBehaviour)transform.root.GetComponent(typeof(UdonBehaviour));
        if (UdonColorController != null)
        {
            ColorLight = (Color)UdonColorController.GetProgramVariable("colorEnabled");
            ColorDark = (Color)UdonColorController.GetProgramVariable("colorDisabled");
        }
    }

    void Interact()
    {
        FullChange();
    }

    public void FullChange()
    {
        MainChange();

        if (SyncTablet && UdonTabletButton != null) Sync_Tablet();
    }

    public void ChangeWithoutSync()
    {
        MainChange();
    }

    void MainChange()
    {
        if (Enabled)
        {
            ChangeColor(this.transform, ColorDark);
            text.text = $"{text.text.Substring(0, text.text.IndexOf(":"))}: Off";
        }
        else
        {
            ChangeColor(this.transform, ColorLight);
            text.text = $"{text.text.Substring(0, text.text.IndexOf(":"))}: On";
        }
        for (int i = 0; i < TargetSwitch.Length; i++)
            if (TargetSwitch[i] != null)
                TargetSwitch[i].SetActive(!Enabled);

        Enabled = !Enabled;
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
