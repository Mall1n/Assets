
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class ChangeMaterial : UdonSharpBehaviour
{

    public GameObject[] Objects = null;
    public Material[] material = null;
    public bool Enabled = false;

    private Text text = null;


    [ColorUsage(true, true)]
    private Color ColorLight;
    [ColorUsage(true, true)]
    private Color ColorDark;


    [Header("Sync with tablet")]

    public bool SyncTablet = false;
    public UdonBehaviour UdonTabletButton = null;

    private void Start()
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
            ChangeText("Off");
            ChangeColor(ColorDark);
            if (material[0] != null)
                for (int i = 0; i < Objects.Length; i++)
                    if (Objects[i] != null)
                        Objects[i].GetComponent<MeshRenderer>().material = material[0];
        }
        else
        {
            ChangeText("On");
            ChangeColor(ColorLight);
            if (material[1] != null)
                for (int i = 0; i < Objects.Length; i++)
                    if (Objects[i] != null)
                        Objects[i].GetComponent<MeshRenderer>().material = material[1];
        }
        Enabled = !Enabled;
    }


    void Sync_Tablet()
    {
        UdonBehaviour udon = (UdonBehaviour)UdonTabletButton.GetProgramVariable("UdonLerp");
        bool left = (bool)udon.GetProgramVariable("left");
        if (!left != Enabled)
            UdonTabletButton.SendCustomEvent("ChangeWithoutSync");
    }

    void ChangeColor(Color color)
    {
        ColorBlock colorBlock = transform.GetComponent<Button>().colors;
        colorBlock.normalColor = color;
        transform.GetComponent<Button>().colors = colorBlock;
    }

    void ChangeText(string s)
    {
        if (text != null)
            text.text = $"{text.text.Substring(0, text.text.IndexOf(":"))}: {s}";
    }

}
