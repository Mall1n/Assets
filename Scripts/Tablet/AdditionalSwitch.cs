
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AdditionalSwitch : UdonSharpBehaviour
{
    public Transform Object = null;

    public void CustomEvent()
    {
        Object.GetComponent<MeshRenderer>().enabled = !Object.GetComponent<MeshRenderer>().enabled;
    }
}
