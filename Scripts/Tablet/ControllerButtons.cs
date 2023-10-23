
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ControllerButtons : UdonSharpBehaviour
{

    [Space]
    public UdonBehaviour UdonLerp = null;
    public bool PreDisable = false;


    public void FullChange()
    {
        Disable();
        UdonLerp.SendCustomEvent("FullChange");
    }

    public void ChangeWithoutSync()
    {
        Disable();
        UdonLerp.SendCustomEvent("ChangeWithoutSync");
    }

    public void LerpObject()
    {
        Disable();
    }

    private void Disable()
    {
        if (PreDisable) UdonLerp.enabled = false;
        UdonLerp.enabled = true;
    }

}
