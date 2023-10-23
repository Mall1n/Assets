
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Loading : UdonSharpBehaviour
{

    private Transform loading = null;

    public float SecondsDelay;

    private void Start()
    {
        loading = transform.Find("Loading");
        //if (loading == null) this.enabled = false;
    }

    public void TurnOn()
    {
        if (loading != null)
        {
            loading.gameObject.SetActive(true);
            SendCustomEventDelayedSeconds("TurnOff", SecondsDelay);
        }
    }

    public void TurnOn5Sec()
    {
        if (loading != null)
        {
            loading.gameObject.SetActive(true);
            SendCustomEventDelayedSeconds("TurnOff", 4.5f);
        }
    }

    public void TurnOff()
    {
        if (loading != null)
        {
            loading.gameObject.SetActive(false);
        }
    }
}
