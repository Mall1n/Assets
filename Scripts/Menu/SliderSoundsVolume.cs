using UnityEngine.UI;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SliderSoundsVolume : UdonSharpBehaviour
{
    public AudioSource[] audioSources = null;
    public bool Non_Proportional = false;

    [Header("Sync Sliders")]

    public bool SyncSliders = false;
    public Transform[] sliders = null;

    private Slider slider = null;
    private float[] values;

    public void Start()
    {
        if (!Non_Proportional)
        {
            values = new float[audioSources.Length];
            for (int i = 0; i < audioSources.Length; i++)
                if (audioSources[i] != null)
                    values[i] = audioSources[i].volume;
        }
        slider = transform.GetComponent<Slider>();
    }

    public void SliderEvent()
    {
        if (slider != null)
        {
            if (!Non_Proportional)
                for (int i = 0; i < audioSources.Length; i++)
                    audioSources[i].volume = slider.value * values[i];
            else
                for (int i = 0; i < audioSources.Length; i++)
                    audioSources[i].volume = slider.value;
            if (SyncSliders && sliders != null)
                for (int i = 0; i < sliders.Length; i++)
                    sliders[i].Find("Button/Slider").GetComponent<Slider>().value = slider.value;
        }
        else Debug.Log("Slider is NULL");
    }
}
