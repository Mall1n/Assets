
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SC_SwitchObject : UdonSharpBehaviour
{

    public bool DependsOnActive = false;
    public Transform[] Transforms = null;


    [Header("Sync with menu")]
    public bool EnableSync = false;
    public Transform buttonSync = null;

    private bool left = true;


    public void FullChange()
    {
        //CheckConflict();

        MainChange();

        Sync_Canvas();
    }

    public void ChangeWithoutSync()
    {
        MainChange();

        //CheckConflict();
    }

    // public void ChangeWithoutConflict()
    // {
    //     Sync_Canvas();

    //     MainChange();
    // }

    private void MainChange()
    {
        left = !left;

        for (int i = 0; i < Transforms.Length; i++)
            if (Transforms[i] != null)
            {
                if (!DependsOnActive) Transforms[i].gameObject.SetActive(!left);
                else Transforms[i].gameObject.SetActive(!Transforms[i].gameObject.activeSelf);
            }
    }

    public void Sync_Canvas()
    {
        if (EnableSync && buttonSync != null) SyncButton();
    }

    void SyncButton()
    {
        UdonBehaviour Udon = (UdonBehaviour)buttonSync.GetComponent(typeof(UdonBehaviour));
        if (Udon != null)
        {
            bool enabled = (bool)Udon.GetProgramVariable("Enabled");
            if (!left != enabled)
                Udon.SendCustomEvent("ChangeWithoutSync");
        }
        else
        {
            Transform t = buttonSync.Find("Button");
            if (t != null)
            {
                Udon = (UdonBehaviour)t.GetComponent(typeof(UdonBehaviour));
                bool enabled = (bool)Udon.GetProgramVariable("Enabled");
                if (!left != enabled)
                    Udon.SendCustomEvent("ChangeWithoutSync");
            }
        }
    }

    // void CheckConflict()
    // {
    //     if (Conflict && udon != null && left)
    //     {

    //         //Networking.SetOwner(Networking.LocalPlayer, udon.gameObject);

    //         var left = udon.GetProgramVariable("syncState");
    //         if (left.GetType() == typeof(bool))
    //             if ((bool)left == false)
    //                 udon.SendCustomEvent("ActivateWOSync");
    //     }
    // }
}
