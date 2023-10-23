
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


public class CanvasColorController : UdonSharpBehaviour
{

    [ColorUsage(true, true)]
    public Color colorLight;
    [ColorUsage(true, true)]
    public Color colorEnabled;
    [ColorUsage(true, true)]
    public Color colorDisabled;

    [ColorUsage(true, true)]
    public Color SliderColor;
    [ColorUsage(true, true)]
    public Color SliderBackgroundLight;
    [ColorUsage(true, true)]
    public Color SliderBackgroundDark;

    [ColorUsage(true, true)]
    public Color StripsColor;

    public Font fontText;

    void Start()
    {

    }
}
