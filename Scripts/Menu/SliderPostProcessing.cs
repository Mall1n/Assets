using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.Rendering.PostProcessing;

public class SliderPostProcessing : UdonSharpBehaviour
{

    public PostProcessVolume postProcess = null;

    [Header("Sync Sliders")]

    public bool SyncSliders = false;
    public Transform[] sliders = null;
    private Slider slider = null;

    public void Start()
    {
        slider = transform.GetComponent<Slider>();
    }

    public void SliderEvent()
    {
        if (slider != null)
        {
            postProcess.weight = slider.value;
            if (SyncSliders && sliders != null)
                for (int i = 0; i < sliders.Length; i++)
                    sliders[i].Find("Button/Slider").GetComponent<Slider>().value = slider.value;
        }
        else Debug.Log("Slider is NULL");
    }

}
