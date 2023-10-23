using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RespawnObjectsByMaster : UdonSharpBehaviour
{

    public GameObject[] objects = null;
    public UdonBehaviour UdonLoading = null;

    private Vector3[] VecPosition;
    private Vector3[] VecRotation;
    private Text text = null;
    private Button button = null;
    private string loading = "";
    private int iter = 0;

    void Start()
    {
        text = transform.GetComponent<Text>();
        button = transform.GetComponent<Button>();
        VecPosition = new Vector3[objects.Length];
        VecRotation = new Vector3[objects.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                VecPosition[i] = new Vector3(
                    objects[i].transform.localPosition.x,
                    objects[i].transform.localPosition.y,
                    objects[i].transform.localPosition.z);
                VecRotation[i] = new Vector3(
                    objects[i].transform.eulerAngles.x,
                    objects[i].transform.eulerAngles.y,
                    objects[i].transform.eulerAngles.z);
            }
        }
        UdonBehaviour UdonColorController = (UdonBehaviour)transform.root.GetComponent(typeof(UdonBehaviour));
        if (button != null && UdonColorController != null)
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.disabledColor = (Color)UdonColorController.GetProgramVariable("colorDisabled");
            button.colors = colorBlock;
        }
    }

    void Interact()
    {
        if (Networking.LocalPlayer.isMaster)
        {
            for (int i = 0; i < objects.Length; i++)
                if (objects[i] != null)
                    Networking.SetOwner(Networking.LocalPlayer, objects[i]);

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Respawn");
        }

    }

    public void Respawn()
    {
        button.interactable = false;
        SendCustomEvent("Loading");
        if (UdonLoading != null)
            UdonLoading.SendCustomEvent("TurnOn5Sec");
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].transform.localPosition = new Vector3(VecPosition[i].x, VecPosition[i].y, VecPosition[i].z);
                objects[i].transform.eulerAngles = new Vector3(VecRotation[i].x, VecRotation[i].y, VecRotation[i].z);
            }
        }
    }


    public void Loading()
    {
        if (loading.Length == 4)
        {
            iter++;
            if (iter == 3)
            {
                text.text = "RespawnObjects";
                iter = 0;
                button.interactable = true;
                loading = "";
                return;
            }
            loading = "";
        }
        text.text = $"RespawningObjects{loading}";
        loading += ".";
        SendCustomEventDelayedSeconds("Loading", 0.5f);
    }

}
